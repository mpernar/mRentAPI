using AIForRentersAPI.Controllers;
using AIForRentersAPI.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AIForRentersAPI.Exceptions.Exception;

namespace AIForRentersAPI.Functionalities
{
    public static class ResponseProcessor
    {
        /// <summary>
        /// Receives instance (object) of ReceivedData class and doing semantic analysis on body of received email as a string.
        /// This method is using Microsoft.Recognizers.Text library for semantic analysis.
        /// </summary>
        /// <param name="receivedData"></param>
        /// <returns>
        /// New request object of class Request with its attributes.
        /// </returns>
        public static void ProcessData(List<ReceivedData> receivedData)
        {
            foreach (ReceivedData receivedDataItem in receivedData)
            {
                // Client e-mail adress
                string emailAddress = receivedDataItem.EmailAddress;

                // Client e-mail subject
                string emailSubject = receivedDataItem.EmailSubject;

                // Client name and surname
                string emailSenderNameAndSurname = receivedDataItem.ClientNameSurname;
                string[] nameAndSurnameSplitted = emailSenderNameAndSurname.Split(' ');
                string name = nameAndSurnameSplitted[0];
                string surname = "";
                if (nameAndSurnameSplitted.Count() == 2)
                {
                    surname = nameAndSurnameSplitted[1];
                }

                //Email body
                string emailBody = receivedDataItem.EmailBody;

                int numberOfPeople = 0;
                Property selectedProperty = null;
                Unit selectedUnit = null;
                DateTime dateFrom = DateTime.Now;
                DateTime dateTo = DateTime.Now;

                try
                {
                    // Processed email body data (Dates and number of people)
                    numberOfPeople = ExtractNumberOfPeople(emailBody);
                    dateTo = CheckYear(DateTime.Parse(ExtractDateTo(emailBody, emailAddress)));
                    dateFrom = CheckYear(DateTime.Parse(ExtractDateFrom(emailBody, emailAddress)));
                }
                catch (Exception ex)
                {
                    if (ex is EmailContentException || ex is InvalidOperationException || ex is FormatException)
                    {
                        string subject = "Insufficient data";
                        string body = $"Dear {name}, \n\nwe are sorry to inform you that you have provided insufficient data in your email request! \nPlease resend your request with all necessary data! \n\nSincerely, \nAIForRenters";
                        EmailSender.SendEmail(subject, body, emailAddress);
                    }
                    return;
                }

                //sending confirmation mail
                if (emailSubject == "Confirmation")
                {
                    ConfirmationEmail(nameAndSurnameSplitted, dateTo, dateFrom);
                }
                else
                {
                    try
                    {
                        //method for extracting property from emailSubject
                        selectedProperty = GetProperty(emailSubject);
                    }
                    catch (Exception ex)
                    {
                        if (ex is EmailContentException || ex is InvalidOperationException)
                        {
                            //sending email if property in subject is invalid
                            string subject = "Invalid property";
                            string body = $"Dear {name}, \n\nyou have sent invalid or nonexistent property name in email subject! \nPlease resend your request with valid property name in email subject! \n\nSincerely, \nAIForRenters";
                            EmailSender.SendEmail(subject, body, emailAddress);
                        }
                        return;
                    }

                    try
                    {
                        //method for extracting unit from emailSubject and numberOfPeople
                        selectedUnit = GetUnit(emailSubject, numberOfPeople);
                    }
                    catch (Exception ex)
                    {
                        if (ex is EmailContentException || ex is InvalidOperationException)
                        {
                            //sending email if there is no unit with requested capacity
                            string subject = "Unavailable unit";
                            string body = $"Dear {name}, \n\nwe are sorry to inform you that there are no available units that have a capacity for the number of people you requested! \n\nSincerely, \nAIForRenters";
                            EmailSender.SendEmail(subject, body, emailAddress);
                        }
                        return;
                    }

                    //creating of new Client object
                    double priceUponRequest = selectedUnit.Price;
                    Client newClient = new Client()
                    {
                        Name = name,
                        Surname = surname,
                        Email = emailAddress
                    };

                    //creating of new Request object
                    Request newRequest = new Request()
                    {
                        Property = selectedProperty.Name,
                        Unit = selectedUnit.Name,
                        FromDate = dateFrom,
                        ToDate = dateTo,
                        NumberOfPeople = numberOfPeople,
                        Client = newClient,
                        Confirmed = false,
                        Processed = false,
                        Sent = false,
                        PriceUponRequest = priceUponRequest,
                        ResponseSubject = "",
                        ResponseBody = ""
                    };

                    //adding new client and request to database
                    using (var context = new AIForRentersDbContext())
                    {

                        //ClientsController clientsController = new ClientsController(context);
                        //var resultClient = clientsController.PostClient(newClient);

                        //RequestsController requestsController = new RequestsController(context);
                        //var resultRequest = requestsController.PostRequest(newRequest);

                        context.Client.Add(newClient);
                        context.Request.Add(newRequest);

                        context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Method for marking request as confirmed upon receiving confirmation email.
        /// </summary>
        /// <param name="nameAndSurnameSplitted"></param>
        /// <param name="dateTo"></param>
        /// <param name="dateFrom"></param>
        private static void ConfirmationEmail(string[] nameAndSurnameSplitted, DateTime dateTo, DateTime dateFrom)
        {
            string name = nameAndSurnameSplitted[0];
            string surname = nameAndSurnameSplitted[1];

            Request requestForConfirmation = null;
            using (var context = new AIForRentersDbContext())
            {
                var query = from request in context.Request
                            where request.Client.Name == name && request.Client.Surname == surname
                            && dateTo == request.ToDate && dateFrom == request.FromDate
                            select request;

                requestForConfirmation = query.First();

                requestForConfirmation.Confirmed = true;

                //RequestsController requestsController = new RequestsController(context);
                //var resultRequestUpdate = requestsController.PutRequest(requestForConfirmation.RequestId, requestForConfirmation);
                
                context.SaveChanges();

                EmailSender.SendEmail("Confirmation", $"Dear {requestForConfirmation.Client.Name}, \n\nYour confirmation is successfully noted! \nWe are looking forward to your arrival! \n\nSincerely, \nAIForRenters", requestForConfirmation.Client.Email);
            }

        }

        /// <summary>
        /// Method that receives DateTime object and checks if year is current year.
        /// If it is not current year it updates the year to current year and returns updated DateTime object.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>DateTime object with current year</returns>
        private static DateTime CheckYear(DateTime dateTime)
        {
            DateTime currentDateTime = DateTime.Now;

            if (dateTime.Year < currentDateTime.Year)
            {
                int diff = currentDateTime.Year - dateTime.Year;

                dateTime = dateTime.AddYears(diff);
                return dateTime;
            }
            else
            {
                return dateTime;
            }

        }

        /// <summary>
        /// Method that receives email subject and returns a property with requested property name.
        /// </summary>
        /// <param name="emailSubject"></param>
        /// <returns>Property object</returns>
        private static Property GetProperty(string emailSubject)
        {
            Property selectedProperty;

            using (var context = new AIForRentersDbContext())
            {
                var queryProperty = from property in context.Property
                                    where property.Name == emailSubject
                                    select property;

                selectedProperty = queryProperty.Single();
            }
            return selectedProperty;
        }

        /// <summary>
        /// Method that receives email subject and extracted number of people and
        /// returns a unit that has a capacity for requested number of people.
        /// </summary>
        /// <param name="emailSubject"></param>
        /// <param name="numberOfPeople"></param>
        /// <returns>Unit object</returns>
        private static Unit GetUnit(string emailSubject, int numberOfPeople)
        {
            Unit selectedUnit;

            using (var context = new AIForRentersDbContext())
            {
                var queryUnit = from unit in context.Unit
                                where unit.Property.Name == emailSubject && unit.Capacity >= numberOfPeople
                                select unit;

                selectedUnit = queryUnit.First();
            }
            return selectedUnit;
        }

        /// <summary>
        /// Method for extracting number of people from email body.
        /// </summary>
        /// <param name="testEmailString"></param>
        /// <returns>Number of people for request</returns>
        private static int ExtractNumberOfPeople(string testEmailString)
        {
            var result = NumberRecognizer.RecognizeNumber(testEmailString, Culture.English);

            int.TryParse(result.First().Resolution["value"].ToString(), out int value);

            if (value.ToString() == null)
            {
                throw new EmailContentException("");
            }
            return value;
        }

        /// <summary>
        /// Method for extracting check in date from email body.
        /// </summary>
        /// <param name="emailBody"></param>
        /// <param name="emailAddress"></param>
        /// <returns>DateTime object representing check in date</returns>
        public static string ExtractDateFrom(string emailBody, string emailAddress)
        {
            string dateFrom = ExtractDates(emailBody).ToList<string>().ToArray()[2];

            if (dateFrom == null)
            {
                throw new EmailContentException("");
            }
            return dateFrom;
        }

        /// <summary>
        /// Method for extracting check out date from email body.
        /// </summary>
        /// <param name="emailBody"></param>
        /// <param name="emailAddress"></param>
        /// <returns>DateTime object representing check out date</returns>
        public static string ExtractDateTo(string emailBody, string emailAddress)
        {
            string dateTo = ExtractDates(emailBody).ToList<string>().ToArray()[3];

            if (dateTo == null)
            {
                throw new EmailContentException("");
            }
            return dateTo;
        }

        /// <summary>
        /// Method for extracting dates from email body.
        /// </summary>
        /// <param name="emailBody"></param>
        /// <returns>List of dates</returns>
        public static Dictionary<string, string>.ValueCollection ExtractDates(string emailBody)
        {
            if (emailBody == null)
            {
                throw new EmailContentException("");
            }
            var result = DateTimeRecognizer.RecognizeDateTime(emailBody, Culture.English);

            var extractFirstLayer = result.First().Resolution.Values.First() as List<Dictionary<string, string>>;

            var dictionaryAsValueCollection = extractFirstLayer.First().Values;
            return dictionaryAsValueCollection;
        }
    }
}
