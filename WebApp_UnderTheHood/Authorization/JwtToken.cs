﻿using Newtonsoft.Json;
using System;


namespace WebApp_UnderTheHood.Authorization
{
    public class JwtToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }


        [JsonProperty("expires_At")]
        public DateTime ExpiresAt { get; set; }
    }
}
