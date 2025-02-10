using Prism.Mvvm;
using System;

namespace WpfPrismFrameworkTemplate.Model
{
    using Prism.Mvvm;
    using System;
    using System.Collections.Generic;

    public class People : BindableBase
    {
        private string _IdName = "";
        private string _name;
        private string _dynasty;
        private string _religion;
        private DateTime _birthDay;
        private DateTime _deathDay;

        // 静态字典，用来存储每个家族的递增计数器
        private static Dictionary<string, int> dynastyCounters = new Dictionary<string, int>();

        public People(string name = "test_name", string dynasty = "test_dynasty", string religion = "test_religion")
        {
            Name = name;
            Dynasty = dynasty;
            Religion = religion;

            // 生成IdName
            IdName = GenerateIdName(dynasty);
        }

        private string GenerateIdName(string dynasty)
        {
            if (!dynastyCounters.ContainsKey(dynasty))
            {
                dynastyCounters[dynasty] = 0;  // 如果没有该家族，初始化计数器
            }

            dynastyCounters[dynasty]++;
            return $"{dynasty}_{dynastyCounters[dynasty]}";  // 生成IdName格式：Dynasty_1, Dynasty_2, ...
        }

        public string IdName
        {
            get => _IdName;
            private set => SetProperty(ref _IdName, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Dynasty
        {
            get => _dynasty;
            set => SetProperty(ref _dynasty, value);
        }

        public string Religion
        {
            get => _religion;
            set => SetProperty(ref _religion, value);
        }

        public DateTime BirthDay
        {
            get => _birthDay;
            set => SetProperty(ref _birthDay, value);
        }

        public DateTime DeathDay
        {
            get => _deathDay;
            set => SetProperty(ref _deathDay, value);
        }
    }

}




