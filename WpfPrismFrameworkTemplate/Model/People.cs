using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WpfPrismFrameworkTemplate.Model
{
    public class People : BindableBase
    {
        private string _idName = "";
        private string _name;
        private string _dynasty;
        private string _religion;
        private string _birthDay;
        private string _deathDay;
        private string _culture;  // 新增culture属性

        // 静态字典，用来存储每个家族的递增计数器
        private static Dictionary<string, int> dynastyCounters = new Dictionary<string, int>();

        public People(string name = "test_name", string dynasty = "test_dynasty", string religion = "test_religion", string culture = "test_culture")
        {
            Name = name;
            Dynasty = dynasty;
            Religion = religion;
            Culture = culture;
            IdName = GenerateIdName(dynasty);
        }

        private string GenerateIdName(string dynasty)
        {
            if (!dynastyCounters.ContainsKey(dynasty))
            {
                dynastyCounters[dynasty] = 0;
            }
            dynastyCounters[dynasty]++;
            return $"{dynasty}_{dynastyCounters[dynasty]}";
        }

        [Browsable(false)]
        public string IdName
        {
            get => _idName;
            private set => SetProperty(ref _idName, value);
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

        public string Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        [Browsable(false)]
        public string BirthDay
        {
            get => _birthDay;
            set => SetProperty(ref _birthDay, value);
        }

        [Browsable(false)]
        public string DeathDay
        {
            get => _deathDay;
            set => SetProperty(ref _deathDay, value);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            // 主要信息
            sb.AppendLine($"{IdName} = {{");
            sb.AppendLine($"\tname = {Name} # a lord");
            sb.AppendLine($"\tdynasty = {Dynasty}");
            sb.AppendLine($"\treligion = {Religion}");

            // 只有在culture不为空时才添加
            if (!string.IsNullOrEmpty(Culture))
            {
                sb.AppendLine($"\tculture = {Culture}");
            }

            // 出生信息
            if (!string.IsNullOrEmpty(BirthDay))
            {
                sb.AppendLine($"\t{BirthDay} = {{");
                sb.AppendLine("\t\tbirth = yes");
                sb.AppendLine("\t}");
            }

            // 死亡信息
            if (!string.IsNullOrEmpty(DeathDay))
            {
                sb.AppendLine($"\t{DeathDay} = {{");
                sb.AppendLine("\t\tdeath = yes");
                sb.AppendLine("\t}");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}