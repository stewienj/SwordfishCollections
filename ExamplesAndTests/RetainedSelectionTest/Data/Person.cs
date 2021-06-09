using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetainedSelectionTest.Data
{
    public class Person : ExtendedNotifyPropertyChanged
    {
        private static Random _random = new Random();

        public static Person GetNewRandom()
        {
            // I was writing a name generator but that was taking too long
            // Just generate 6 random letters for first and last name
            char[] firstName = new char[6];
            char[] lastName = new char[6];
            for (int i = 0; i < 6; ++i)
            {
                firstName[i] = (char)(_random.Next(26) + 'a');
                lastName[i] = (char)(_random.Next(26) + 'a');
            }
            int age = _random.Next(123);
            GenderType gender = (GenderType)_random.Next(3);
            return new Person { FirstName = new string(firstName), LastName = new string(lastName), Age = age, Gender = gender };
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private int _age = 0;
        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        private GenderType _gender = GenderType.Unspecified;
        public GenderType Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }
    }
}
