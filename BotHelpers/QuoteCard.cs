﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChatbotCustomerOnboarding.DataModel;
using LaYumba.Functional;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ChatbotCustomerOnboarding.ErrorValidation.Validator;

/* Class to Interact with Rest API services/
   ///-----------------------------------------------------------------
   ///   Namespace:      <ChatbotCustomerOnboarding>
   ///   Class:          <QuoteCard>
   ///   Description:    <Card which displays customer Quote and Invoke rest api to create customers>
   ///   Author:         <Vignesh Chandran balan>                    
 ///-----------------------------------------------------------------*/

namespace ChatbotCustomerOnboarding.BotHelpers
{
    using static F;
    public class QuoteCard : GenericHelpers
    {
        public async static Task<string> GetQuote()
        {
            IAPIHelper Invoke = new APIHelper();
            Option<HttpResponseMessage> getQuoteRate = await Invoke.GetAPI("https://ai-customer-onboarding-dev.azurewebsites.net/api/InsuranceQuote/", $"{CreateCustomer.Instance.ZipCode}", HttpStatusCode.OK);
            return
                await getQuoteRate.Match(
                    None: () => ReportNotFound(),
                    Some: async (r) => (await ConvertToQuoteObjectAsync(r)).Match(
                        None: () => "JSON object could not be converted",
                        Some: (q) => q
                    )
                );
        }

        private static Func<Task<string>> ReportNotFound = async () => $"{IsInvalid} Quote Not Found for the provided ZipCode";
        private static Func<Task<string>> ReportError = async () => "An error occurred.";

        private static Func<HttpResponseMessage, Task<Option<string>>> ConvertToQuoteObjectAsync = async (request) =>
        {
            string getQuoteJson = await request.Content.ReadAsStringAsync();
            List<string> errors = new List<string>();
            GetQuote getQuote = JsonConvert.DeserializeObject<GetQuote>(
                getQuoteJson.ToString(),
                new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                });
            CustomerPolicy.Instance.TotalAmount = (Convert.ToDouble(getQuote.Quote) * 12);
            return errors.Count == 0 ? Some(getQuote.Quote) : None;
        };

        public async static Task<string> CreateCustomerRentersInsurance()
        {
            IAPIHelper Invoke = new APIHelper();
            JObject customerRecord = JObject.Parse(File.ReadAllText(Path.Combine(".", "JsonTemplate", "CreateCustomer.json")));
            //TODO 
            customerRecord["zipCode"] = CreateCustomer.Instance.ZipCode;
            customerRecord["firstName"] = CreateCustomer.Instance.FirstName;
            customerRecord["middleName"] = CreateCustomer.Instance.MiddleName;
            customerRecord["lastName"] = CreateCustomer.Instance.LastName;
            customerRecord["addressLine1"] = $"{CreateCustomer.Instance.AddressLine1}";
            customerRecord["addressLine2"] = $"{CreateCustomer.Instance.AddressLine2}";
            customerRecord["state"] = $"{CreateCustomer.Instance.State}";
            customerRecord["mobileNumber"] = CreateCustomer.Instance.MobileNumber;
            customerRecord["emailAddress"] = CreateCustomer.Instance.EmailAddress;
            Option<HttpResponseMessage> getCustomer = await Invoke.PostAPI("https://ai-customer-onboarding-dev.azurewebsites.net/api/Customer", "", HttpStatusCode.Created, customerRecord.ToString());
            string getQuoteJson = await getCustomer.Match(
                None: () => ReportError(),
                Some: async (r) => await r.Content.ReadAsStringAsync()
            );
            Option<GetCustomer> customer = ConvertToObject<GetCustomer>(getQuoteJson);
            CreateCustomer.Instance.CustomerId = customer.Match(
                None: () => 0,
                Some: (c) => Convert.ToInt32(c.CustomerId)
            );
            return customer.Match(
                None: () => "An error occurred. Your information has not been updated.",
                Some: (c) => c.CustomerId.ToString()
            );
        }

        public async static Task<string> AddCoverage()
        {
            IAPIHelper Invoke = new APIHelper();
            JObject customerRecord = JObject.Parse(File.ReadAllText(Path.Combine(".", "JsonTemplate", "AddCoverage.json")));
            //TODO
            customerRecord["customerId"] = CreateCustomer.Instance.CustomerId;
            customerRecord["personalPropertyCoverage"] = CustomerCoverage.Instance.PersonalLiabilityLimit;
            customerRecord["propertyDeduction"] = CustomerCoverage.Instance.PropertyDeduction;
            customerRecord["personalLiabilityLimit"] = CustomerCoverage.Instance.PersonalLiabilityLimit;
            customerRecord["damageToPropertyOfOthers"] = CustomerCoverage.Instance.DamageToPropertyOfOthers;

            Option<HttpResponseMessage> getCustomer = await Invoke.PostAPI("https://ai-customer-onboarding-dev.azurewebsites.net/api/Coverage", "", HttpStatusCode.Created, customerRecord.ToString());
            string getQuoteJson =
                await getCustomer.Match(
                    None: () => ReportNotFound(),
                    Some: async (r) => await r.Content.ReadAsStringAsync()
                );
            Option<CustomerCoverage> getCustomerInfo = ConvertToObject<CustomerCoverage>(getQuoteJson);
            return getCustomerInfo.Match(
                None: () => "",
                Some: (c) => c.ToString()
            );
        }

        public async static Task CreatePolicy()
        {
            IAPIHelper Invoke = new APIHelper();
            JObject customerRecord = JObject.Parse(File.ReadAllText(Path.Combine(".", "JsonTemplate", "CreatePolicy.json")));
            //TODO
            customerRecord["customerId"] = CreateCustomer.Instance.CustomerId;
            customerRecord["policyEffectiveDate"] = CustomerPolicy.Instance.PolicyEffectiveDate;
            customerRecord["policyExpiryDate"] = CustomerPolicy.Instance.PolicyExpiryDate;
            customerRecord["paymentOption"] = CustomerPolicy.Instance.PaymentOption;
            customerRecord["totalAmount"] = CustomerPolicy.Instance.TotalAmount;


            Option<HttpResponseMessage> getCustomer = await Invoke.PostAPI("https://ai-customer-onboarding-dev.azurewebsites.net/api/Coverage", "", HttpStatusCode.Created, customerRecord.ToString());
            string getQuoteJson =
                await getCustomer.Match(
                    None: () => ReportNotFound(),
                    Some: async (r) => await r.Content.ReadAsStringAsync()
                );
            Option<GetCustomer> getCustomerInfo = ConvertToObject<GetCustomer>(getQuoteJson);
            CreateCustomer.Instance.PolicyNumber = getCustomerInfo.Match(
                None: () => 0,
                Some: (c) => Convert.ToInt32(c.PolicyNumber)
            );
        }

        public static Attachment QuoteSummary()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = $"Renters Insurance Quote - EstimatedTotal: {GetCustomer.Instance.Quote} per month;",
                Subtitle = "Personal Details",
                Text = $"Name:{CreateCustomer.Instance.FirstName} {CreateCustomer.Instance.MiddleName} {CreateCustomer.Instance.LastName}; Address: {CreateCustomer.Instance.AddressLine1}, {CreateCustomer.Instance.AddressLine2},State: {CreateCustomer.Instance.State}, ZipCode: {CreateCustomer.Instance.ZipCode}",
                Buttons = new[] {new CardAction(
                        ActionTypes.MessageBack, $"Ok", value: "QuoteCardOk"
                        ) }

            };

            return thumbnailCard.ToAttachment();
        }

        public static Attachment RentersInsuranceSummary()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = $"Renters insurance has been successfully Purchased:",
                Subtitle = $"Customer ID: {CreateCustomer.Instance.CustomerId},PolicyNo: {CreateCustomer.Instance.PolicyNumber}",
                Text = $"Name:{CreateCustomer.Instance.FirstName} {CreateCustomer.Instance.MiddleName} {CreateCustomer.Instance.LastName}; " +
                $"Address: {CreateCustomer.Instance.AddressLine1}, " +
                $"{CreateCustomer.Instance.AddressLine2}," +
                $"State: {CreateCustomer.Instance.State}, " +
                $"ZipCode: {CreateCustomer.Instance.ZipCode}," +
                $"Total Amount:{CustomerPolicy.Instance.TotalAmount}",
                Buttons = new[] {new CardAction(
                        ActionTypes.MessageBack, $"Ok", value: "CustomerOk"
                        ) }
            };

            return thumbnailCard.ToAttachment();
        }

        public static Attachment EmailConfirmation()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = $"An email has been sent with quote information:",
                Subtitle = $"Customer ID:{CreateCustomer.Instance.CustomerId}, {CreateCustomer.Instance.EmailAddress}",
                Text = $"Name:{CreateCustomer.Instance.FirstName} {CreateCustomer.Instance.MiddleName} {CreateCustomer.Instance.LastName}; Address: {CreateCustomer.Instance.AddressLine1}, {CreateCustomer.Instance.AddressLine2},State: {CreateCustomer.Instance.State}, ZipCode: {CreateCustomer.Instance.ZipCode}",
                Buttons = new[] {new CardAction(
                        ActionTypes.MessageBack, $"Ok", value: "emailConfirmOk"
                        ) }
            };

            return thumbnailCard.ToAttachment();
        }
    }
}




