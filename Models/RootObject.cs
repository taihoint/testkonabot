namespace Bot_Application1.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class RootObject
    {
        public List<Search> items { get; set; }
        public int display { get; set; }
    }
}