﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OTPVerificationSample
{
    public partial class MainPage : ContentPage
    {
        int otpNo = 0;
        public MainPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<string>(this, "ReceivedOTP", (message) =>
             {
                 string[] words = message.Split();
                 foreach (string item in words.ToList())
                 {
                     var isNumeric = int.TryParse(item, out int n);
                     if (isNumeric)
                     {
                         otp.IsVisible = true;
                         mobileNumber.IsVisible = false;
                         otp.Text = item;
                         btnNext.Text = "Login";
                         //DisplayAlert("Message", $"OTP is {item}", "Ok");
                         break;
                     }
                 }
             });
        }

        private async void SendOTP(object sender, EventArgs e)
        {

            // send a GET request  
            string countryCode = "91";  // two digit only

            OTPModel model = new OTPModel();
            model.sender = "SOCKET";
            model.route = "4";
            model.country = countryCode;
            string hashKey = System.Web.HttpUtility.UrlEncode(DependencyService.Get<IHashService>().GenerateHashkey());
            otpNo = GenerateOTP();
            string message = $"<#> Your OTP is {otp} {hashKey}";
            List<string> numbers = new List<string> { mobileNumber.Text };
            model.sms.Add(new Sms { message = message, to = numbers });

            var client = new RestClient($"https://api.msg91.com/api/v2/sendsms?country={countryCode}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authkey", "281817ABVlLRzJt5d0a30e2");
            string jsonData = JsonConvert.SerializeObject(model);
            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                DependencyService.Get<IHashService>().StartSMSRetriverReceiver();
                OTPResponse resp = JsonConvert.DeserializeObject<OTPResponse>(response.Content);
                if (resp.type == "success")
                {
                    await DisplayAlert("Message", $"An OTP send to {numbers[0]}", "Ok");
                }
                else
                {
                    // handle if sms failed to send
                    await DisplayAlert("Message", resp.message, "Ok");
                }
            }
            else
            {
                DisplayAlert("Message", response.ErrorMessage, "Ok");
            }
        }
        public int GenerateOTP()
        {
            return new Random().Next(1000, 9999);
        }
    }
}
