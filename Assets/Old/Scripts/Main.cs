using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Main : MonoBehaviour
{
    public string str_a, str_b, str_result;
    public int a, b, result;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        str_result = NBITADDER(4, str_a, str_b);
        a = bits_to_int(str_a);
        b = bits_to_int(str_b);
        result = bits_to_int(str_result);
    }

    // GATES

        public (bool, bool) FULL_ADDER(bool _a, bool _b, bool _c)
        {
            // bool a_b_xor = XOR(_a, _b);
            // bool xor_and = AND(a_b_xor, _c);
            // bool a_b_and = AND(_a, _b);

            return (XOR(XOR(_a, _b), _c), OR(AND(XOR(_a, _b), _c), AND(_a, _b)));
        }

        public string NBITADDER(int n_bit_adder, string _str_a, string _str_b)
        {
            string _result="";
            bool prev_carry=false;
            for(int i=0; i<n_bit_adder; i++)
            {
                var op = FULL_ADDER(str_n_to_bool(_str_a, i), str_n_to_bool(_str_b, i), prev_carry);
                _result += op.Item1==true?"1":"0";
                prev_carry = op.Item2;

                if(i==n_bit_adder-1)
                {
                    _result += op.Item2==true?"1":"0";
                    _result = Reverse(_result);
                }
            }
            return _result;
        }

        public int bits_to_int(string a)
        {
            float _decimal = 0;
            for(int i=0; i<a.Length; i++)
            {
                _decimal+=(float)Int32.Parse(a[a.Length-1-i].ToString())*(float)Math.Pow(2, i);
            }
            return (int)_decimal;
        }

    // GATES
    
        public bool AND(bool a, bool b) { return a && b; }
        public bool OR(bool a, bool b) { return a || b; }
        public bool NOT(bool a) { return !a; }
        public bool XOR(bool a, bool b) { return OR(AND(a, NOT(b)), AND(NOT(a), b)); }

    // GATES

    // CUSTOM FUNCS

        public int n(int _n, int i){ return (_n/(int)Math.Pow(10,i))%10; }

        public string Reverse(string text)
        {
            char[] cArray = text.ToCharArray();
            string reverse = String.Empty;
            for (int i = cArray.Length - 1; i > -1; i--)
            {
                reverse += cArray[i];
            }
            return reverse;
        }

        public bool str_n_to_bool(string _str, int i) { return Convert.ToBoolean(_str[_str.Length-i-1].ToString()=="0"?"False":"True"); }

    // CUSTOM FUNCS
}
