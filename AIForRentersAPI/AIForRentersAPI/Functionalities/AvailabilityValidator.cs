using AIForRentersAPI.Models;
using MailKit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AIForRentersAPI.Functionalities
{
    public static class AvailabilityValidator
    {
        /// <summary>
        /// Method receives a list of Request objects and checks for availabilitty of the requestetd property
        /// </summary>
        /// <param name="requests"></param>
        public static void CheckForAvailability(List<Request> requests)
        {
            //storing unprocessed requests to a list
            List<Request> unprocessed = new List<Request>();
            foreach (var item in requests)
            {
                if (!item.Processed)
                {
                    unprocessed.Add(item);
                }
            }

            foreach (var req in unprocessed)
            {
                //method to check if the interval is from Saturday to Saturday
                List<DateTime> newDatesDays = CheckDays(req);

                Availability availability = null;

                string type = "";

                using (var context = new AIForRentersDbContext())
                {
                    //if newDatesDays list is empty the interval is from Saturday to Saturday
                    if (newDatesDays == null)
                    {
                        var queryAvailability = from a in context.Availability
                                                from u in context.Unit
                                                where req.Unit == u.Name && a.UnitId == u.UnitId && a.FromDate == req.FromDate && a.ToDate == req.ToDate
                                                select a;

                        try
                        {
                            availability = queryAvailability.FirstOrDefault();
                        }
                        catch (Exception)
                        {

                        }

                        //if availability is null then the slot is available
                        if (availability == null)
                        {
                            type = "available";
                            Unit resultUnit = null;
                            using (var context1 = new AIForRentersDbContext())
                            {
                                var queryUnit = from unit in context1.Unit
                                                where unit.Name == req.Unit && unit.Property.Name == req.Property
                                                select unit;



                                resultUnit = queryUnit.Single();
                            }

                            //method for calculating total price
                            double totalPrice = CalculateTotalPrice(req);

                            //adding availability to database
                            Availability addAvailability = new Availability();
                            addAvailability.AddAvailability(resultUnit, req.FromDate, req.ToDate);

                            //fetching email template
                            EmailTemplate email = FetchAndCustomizeEmailTemplate(type, req.Property, req.ToDate, req.FromDate, req.Client.Name, req.FromDate, req.FromDate, totalPrice);

                            string emailBody = email.TemplateContent;
                            string emailSubject = email.Name;

                            //updating request response
                            Request request = new Request();
                            request.UpdateRequest(req, emailBody, emailSubject);
                        }
                        //if availability is not null the slot is not available
                        else
                        {
                            //method for checking new availability
                            List<DateTime> newDatesUnavailable = CheckAnotherAvailability(req);

                            type = "unavailable";
                            Unit resultUnit = null;
                            using (var context2 = new AIForRentersDbContext())
                            {
                                var queryUnit = from unit in context2.Unit
                                                where unit.Name == req.Unit && unit.Property.Name == req.Property
                                                select unit;

                                resultUnit = queryUnit.Single();
                            }

                            //if newDatesUnavailable list is not empty we are fetching template with suggestion
                            if (newDatesUnavailable != null)
                            {
                                EmailTemplate email = FetchAndCustomizeEmailTemplate(type, req.Property, req.ToDate, req.FromDate, req.Client.Name, newDatesUnavailable[0], newDatesUnavailable[1]);

                                string emailBody = email.TemplateContent;
                                string emailSubject = email.Name;

                                Request request = new Request();
                                request.UpdateRequest(req, emailBody, emailSubject);
                            }
                            //if newDatesUnavailable list is empty we are fetching template with no suggestion
                            else
                            {
                                type = "unavailableWeekBeforeAfter";
                                EmailTemplate email = FetchAndCustomizeEmailTemplate(type, req.Property, req.ToDate, req.FromDate, req.Client.Name, DateTime.Now, DateTime.Now);

                                string emailBody = email.TemplateContent;
                                string emailSubject = email.Name;

                                Request request = new Request();
                                request.UpdateRequest(req, emailBody, emailSubject);
                            }
                        }
                    }
                    //if days are not from Saturday until Saturday we fetch template 
                    //that suggest nearest Saturday-Saturday interval
                    else
                    {
                        type = "daysError";
                        var queryAvailability = from a in context.Availability
                                                from u in context.Unit
                                                where req.Unit == u.Name && a.UnitId == u.UnitId && a.FromDate == newDatesDays[0] && a.ToDate == newDatesDays[1]
                                                select a;

                        try
                        {
                            availability = queryAvailability.FirstOrDefault();
                        }
                        catch (Exception)
                        {

                        }

                        EmailTemplate email = FetchAndCustomizeEmailTemplate(type, req.Property, req.ToDate, req.FromDate, req.Client.Name, newDatesDays[0], newDatesDays[1]);

                        string emailBody = email.TemplateContent;
                        string emailSubject = email.Name;

                        Request request = new Request();
                        request.UpdateRequest(req, emailBody, emailSubject);
                    }
                }
            }
        }

        /// <summary>
        /// Method that fetches and customizes email template
        /// </summary>
        /// <param name="type"></param>
        /// <param name="property"></param>
        /// <param name="dateTo"></param>
        /// <param name="dateFrom"></param>
        /// <param name="name"></param>
        /// <param name="newDateTo"></param>
        /// <param name="newDateFrom"></param>
        /// <param name="totalPrice"></param>
        /// <returns>EmailTemplate object</returns>
        public static EmailTemplate FetchAndCustomizeEmailTemplate(string type, string property, DateTime dateTo, DateTime dateFrom, string name, DateTime newDateTo, DateTime newDateFrom, double totalPrice = 0)
        {
            using (var context = new AIForRentersDbContext())
            {
                //unit is available
                if (type == "available")
                {
                    var getAvailableTemp = (from t in context.EmailTemplate
                                            where t.Name == "Available unit"
                                            select new { t }).FirstOrDefault();
                    EmailTemplate emailTemp = getAvailableTemp.t;
                    EmailTemplate email = new EmailTemplate();
                    email.Name = emailTemp.Name;
                    email.TemplateContent = emailTemp.TemplateContent.Replace("{Name}", name).Replace("{Property}", property).Replace("{DateTo}", dateTo.ToString()).Replace("{DateFrom}", dateFrom.ToString()).Replace("{Price}", totalPrice.ToString());
                    return email;
                }
                //unit is not available
                else if (type == "unavailable")
                {
                    var getUnavailableTemp = (from t in context.EmailTemplate
                                              where t.Name == "Unavailable unit"
                                              select new { t }).FirstOrDefault();
                    EmailTemplate emailTemp = getUnavailableTemp.t;
                    EmailTemplate email = new EmailTemplate();
                    email.Name = emailTemp.Name;
                    email.TemplateContent = emailTemp.TemplateContent.Replace("{Name}", name).Replace("{Property}", property).Replace("{DateTo}", dateTo.ToString()).Replace("{DateFrom}", dateFrom.ToString()).Replace("{NewDateTo}", newDateFrom.ToString()).Replace("{NewDateFrom}", newDateTo.ToString());
                    return email;
                }
                //the desired time slot is not from Saturday until Saturday
                else if (type == "daysError")
                {
                    var getUnavailableTemp = (from t in context.EmailTemplate
                                              where t.Name == "Invalid request"
                                              select new { t }).FirstOrDefault();
                    EmailTemplate emailTemp = getUnavailableTemp.t;
                    EmailTemplate email = new EmailTemplate();
                    email.Name = emailTemp.Name;
                    email.TemplateContent = emailTemp.TemplateContent.Replace("{Name}", name).Replace("{Property}", property).Replace("{DateTo}", dateTo.ToString()).Replace("{DateFrom}", dateFrom.ToString()).Replace("{NewDateTo}", newDateFrom.ToString()).Replace("{NewDateFrom}", newDateTo.ToString());
                    return email;
                }
                //there are na available time units in the desired time or week before or week later
                else if (type == "unavailableWeekBeforeAfter")
                {
                    var getUnavailableTemp = (from t in context.EmailTemplate
                                              where t.Name == "Unavailable unit with no recommendation"
                                              select new { t }).FirstOrDefault();
                    EmailTemplate emailTemp = getUnavailableTemp.t;
                    EmailTemplate email = new EmailTemplate();
                    email.Name = emailTemp.Name;
                    email.TemplateContent = emailTemp.TemplateContent.Replace("{Name}", name).Replace("{Property}", property).Replace("{DateTo}", dateTo.ToString()).Replace("{DateFrom}", dateFrom.ToString());
                    return email;
                }
                return null;
            }
        }

        /// <summary>
        /// Method that receives Request object and checks if the dates are from Saturday until Saturday
        /// </summary>
        /// <param name="req"></param>
        /// <returns>List of DateTime objects representing new check in and check out dates</returns>
        private static List<DateTime> CheckDays(Request req)
        {
            List<DateTime> newDates = new List<DateTime>(2);

            DayOfWeek dayFrom = req.FromDate.DayOfWeek;
            DayOfWeek dayTo = req.ToDate.DayOfWeek;

            if (dayFrom == DayOfWeek.Saturday && dayTo == DayOfWeek.Saturday)
            {
                return null;
            }

            int diffDayFrom = 0;
            int diffDayTo = 0;
            //both starting date and ending date are not Saturday
            if (dayFrom != DayOfWeek.Saturday && dayTo != DayOfWeek.Saturday)
            {
                diffDayFrom = DayOfWeek.Saturday - dayFrom;
                diffDayTo = DayOfWeek.Saturday - dayTo;
            }
            //starting date is not Saturday
            else if (dayFrom != DayOfWeek.Saturday && dayTo == DayOfWeek.Saturday)
            {
                diffDayFrom = DayOfWeek.Saturday - dayFrom - 7;
                diffDayTo = 0;
            }
            //ending date is not Saturday
            else if (dayFrom == DayOfWeek.Saturday && dayTo != DayOfWeek.Saturday)
            {
                diffDayFrom = 0;
                diffDayTo = DayOfWeek.Saturday - dayTo;
            }

            DateTime newFromDate = req.FromDate.AddDays(diffDayFrom);
            DateTime newToDate = req.ToDate.AddDays(diffDayTo);

            newDates.Add(newFromDate);
            newDates.Add(newToDate);

            return newDates;
        }

        /// <summary>
        /// Method that checks if there are available slots before or after the desired time slot
        /// </summary>
        /// <param name="req"></param>
        /// <returns>List of DateTime objects representing new check in and check out dates</returns>
        private static List<DateTime> CheckAnotherAvailability(Request req)
        {
            List<DateTime> newDates = new List<DateTime>(2);

            DateTime newFromDateOneWeekLater = req.FromDate.AddDays(7);
            DateTime newToDateOneWeekLater = req.ToDate.AddDays(7);

            Availability availability = null;

            using (var context = new AIForRentersDbContext())
            {
                var queryAvailability = from a in context.Availability
                                        from u in context.Unit
                                        where req.Unit == u.Name && a.UnitId == u.UnitId && a.FromDate == newFromDateOneWeekLater && a.ToDate == newToDateOneWeekLater
                                        select a;

                try
                {
                    availability = queryAvailability.FirstOrDefault();
                }
                catch (Exception)
                {

                }

                if (availability == null)
                {
                    newDates.Add(newFromDateOneWeekLater);
                    newDates.Add(newToDateOneWeekLater);
                }
            }

            DateTime newFromDateOneWeekSooner = req.FromDate.AddDays(-7);
            DateTime newToDateOneWeekSooner = req.ToDate.AddDays(-7);

            using (var context = new AIForRentersDbContext())
            {
                var queryAvailability = from a in context.Availability
                                        from u in context.Unit
                                        where req.Unit == u.Name && a.UnitId == u.UnitId && a.FromDate == newFromDateOneWeekSooner && a.ToDate == newToDateOneWeekSooner
                                        select a;

                try
                {
                    availability = queryAvailability.FirstOrDefault();
                }
                catch (Exception)
                {

                }

                if (availability == null)
                {
                    newDates.Add(newFromDateOneWeekSooner);
                    newDates.Add(newToDateOneWeekSooner);
                }
            }

            if (newDates.Count > 0)
            {
                return newDates;
            }
            else
            {
                return null;
            }


        }

        /// <summary>
        /// Method that calculates total price of reservation
        /// </summary>
        /// <param name="req"></param>
        /// <returns>Total price of reservation</returns>
        private static double CalculateTotalPrice(Request req)
        {
            double dailyPrice = req.PriceUponRequest;

            int daysReserved = (req.ToDate - req.FromDate).Days;

            double totalPrice = daysReserved * dailyPrice;

            return totalPrice;
        }
    }
}