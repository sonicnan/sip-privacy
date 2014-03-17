using System;
using System.Collections;
using System.Text;

namespace Security
{
    public class GeneratePassword
    {
        private static readonly char[] _Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _Numbers = "1234567890".ToCharArray();
        private static readonly char[] _Symbols = "!@#$%^&*.?".ToCharArray();

        int _MinimumLength, _MaximumLength;
        bool _IncludeUpper, _IncludeLower, _IncludeNumber, _IncludeSpecial;

        string[] _CharacterTypes;

        enum CharacterType
        {
            Uppercase,
            Lowercase,
            Special,
            Number
        }

        public bool IncludeUpper
        {
            get
            {
                return _IncludeUpper;
            }
            set
            {
                _IncludeUpper = value;
            }
        }

        public bool IncludeLower
        {
            get
            {
                return _IncludeLower;
            }
            set
            {
                _IncludeLower = value;
            }
        }

        public bool IncludeNumber
        {
            get
            {
                return _IncludeNumber;
            }
            set
            {
                _IncludeNumber = value;
            }
        }

        public bool IncludeSpecial
        {
            get
            {
                return _IncludeSpecial;
            }
            set
            {
                _IncludeSpecial = value;
            }
        }

        public int MinimumLength
        {
            get
            {
                return _MinimumLength;
            }
            set
            {
                if (value > _MaximumLength)
                {
                    throw new ArgumentOutOfRangeException("MinimumLength must be greater than MaximumLength");
                }
                _MinimumLength = value;
            }
        }

        public int MaximumLength
        {
            get
            {
                return _MaximumLength;
            }
            set
            {
                if (value < _MinimumLength)
                {
                    throw new ArgumentOutOfRangeException("MaximumLength must be greater than MinimumLength");
                }
                _MaximumLength = value;
            }
        }

        public GeneratePassword()
        {
            _MinimumLength = 6;
            _MaximumLength = 20;
            _IncludeSpecial = false;
            _IncludeNumber = true;
            _IncludeUpper = false;
            _IncludeLower = true;
        }

        public GeneratePassword(bool includeSpecial, bool includeNumber, bool includeUpper, bool includeLower)
        {
            _IncludeNumber = includeNumber;
            _IncludeSpecial = includeSpecial;
            _IncludeUpper = includeUpper;
            _IncludeLower = includeLower;
        }

        public string Create()
        {
            _CharacterTypes = getCharacterTypes();
            StringBuilder password = new StringBuilder(_MaximumLength);
            int currentPasswordLength = RNGRandom.RandomNumber(_MaximumLength);
            if (currentPasswordLength < _MinimumLength)
            {
                currentPasswordLength = _MinimumLength;
            }

            for (int i = 0; i < currentPasswordLength; i++)
            {
                password.Append(getCharacter());
            }
            return password.ToString();
        }

        private string[] getCharacterTypes()
        {
            ArrayList characterTypes = new ArrayList();
            foreach (string characterType in Enum.GetNames(typeof(CharacterType)))
            {
                CharacterType currentType = (CharacterType)Enum.Parse(typeof(CharacterType), characterType, false);
                bool addType = false;
                switch (currentType)
                {
                    case CharacterType.Lowercase:
                        addType = IncludeLower;
                        break;
                    case CharacterType.Number:
                        addType = IncludeNumber;
                        break;
                    case CharacterType.Special:
                        addType = IncludeSpecial;
                        break;
                    case CharacterType.Uppercase:
                        addType = IncludeUpper;
                        break;
                }
                if (addType)
                {
                    characterTypes.Add(characterType);
                }
            }
            return (string[])characterTypes.ToArray(typeof(string));
        }


        private string getCharacter()
        {
            string characterType = _CharacterTypes[RNGRandom.RandomNumber(_CharacterTypes.Length)];
            CharacterType typeToGet = (CharacterType)Enum.Parse(typeof(CharacterType), characterType, false);
            switch (typeToGet)
            {
                case CharacterType.Lowercase:
                    return _Letters[RNGRandom.RandomNumber(_Letters.Length)].ToString().ToLower();
                case CharacterType.Uppercase:
                    return _Letters[RNGRandom.RandomNumber(_Letters.Length)].ToString().ToUpper();
                case CharacterType.Number:
                    return _Numbers[RNGRandom.RandomNumber(_Numbers.Length)].ToString();
                case CharacterType.Special:
                    return _Symbols[RNGRandom.RandomNumber(_Symbols.Length)].ToString();
            }
            return null;
        }
    }
}
