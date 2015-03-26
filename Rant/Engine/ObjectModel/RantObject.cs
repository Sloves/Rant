﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rant.Engine.ObjectModel
{
    public class RantObject
    {
        /// <summary>
        /// No
        /// </summary>
        public static readonly RantObject No = new RantObject();

        private double _number = 0;
        private string _string = null;
        private List<RantObject> _list = null;
        private bool _boolean = false;
        private RantPattern _pattern = null;

        /// <summary>
        /// The type of the object.
        /// </summary>
        public RantObjectType Type { get; private set; } = RantObjectType.No;

        /// <summary>
        /// Creates a No object.
        /// </summary>
        public RantObject()
        {
        }

        public RantObject(List<RantObject> list)
        {
            if (list != null) _list = list;
        }

        public RantObject(bool boolean)
        {
            Type = RantObjectType.Boolean;
            _boolean = boolean;
        }

        public RantObject(string str)
        {
            if (str == null) return;
            _string = str;
        }

        public RantObject(double num)
        {
            Type = RantObjectType.Number;
            _number = num;
        }

        public RantObject(object obj)
        {
            if (obj == null) return;

            if (obj is string)
            {
                _string = obj.ToString();
                Type = RantObjectType.String;
            }
            else if (obj is bool)
            {
                _boolean = (bool)obj;
                Type = RantObjectType.Boolean;
            }
            else if (IsNumber(obj))
            {
                _number = (double)obj;
                Type = RantObjectType.Number;
            }
            else if (obj is List<RantObject>)
            {
                _list = (List<RantObject>)obj;
                Type = RantObjectType.List;
            }
            else if (obj.GetType().IsArray)
            {
                _list = ((object[])obj).Select(o => new RantObject(o)).ToList();
                Type = RantObjectType.List;
            }
            else if (obj is RantPattern)
            {
                _pattern = (RantPattern)obj;
            }

        }

        public RantObject ConvertTo(RantObjectType type)
        {
            if (Type == type) return Clone();

            switch (type)
            {
                case RantObjectType.String:
                    {
                        switch (Type)
                        {
                            case RantObjectType.Boolean:
                                return new RantObject(_boolean.ToString());
                            case RantObjectType.Number:
                                return new RantObject(_number.ToString());
                            case RantObjectType.Pattern:
                                return new RantObject(_pattern.Code);
                            case RantObjectType.List:
                                {
                                    var sb = new StringBuilder();
                                    bool first = true;
                                    sb.Append("(");
                                    foreach (var rantObject in _list)
                                    {
                                        if (first)
                                        {
                                            first = false;
                                            sb.Append(", ");
                                        }

                                        sb.Append(rantObject);
                                    }
                                    sb.Append(")");
                                    return new RantObject(sb.ToString());
                                }
                        }
                        break;
                    }
                case RantObjectType.Number:
                    {
                        switch (Type)
                        {
                            case RantObjectType.Boolean:
                                return new RantObject(_boolean ? 1 : 0);
                            case RantObjectType.String:
                                {
                                    double num;
                                    return double.TryParse(_string, out num) ? new RantObject(num) : No;
                                }
                        }
                        break;
                    }
                case RantObjectType.Boolean:
                    {
                        switch (Type)
                        {
                            case RantObjectType.Number:
                                return new RantObject(_number != 0);
                            case RantObjectType.String:
                                {
                                    var bstr = _string.ToLower().Trim();
                                    switch (bstr)
                                    {
                                        case "true":
                                            return new RantObject(true);
                                        case "false":
                                            return new RantObject(false);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case RantObjectType.List:
                    {
                        return new RantObject(new List<RantObject> { this });
                    }
            }

            return No;
        }

        public RantObject Clone()
        {
            return new RantObject
            {
                _boolean = _boolean,
                _list = _list?.ToList(), // Create a copy of the list
                _number = _number,
                _pattern = _pattern,
                _string = _string,
                Type = Type
            };
        }

        public override string ToString()
        {
            switch (Type)
            {
                case RantObjectType.Boolean:
                    return _boolean.ToString();
                case RantObjectType.String:
                    return "\"\{_string}\"";
                case RantObjectType.No:
                    return "no";
                case RantObjectType.Number:
                    return _number.ToString();
                case RantObjectType.Pattern:
                    return "$'\{_pattern.Code}'";
                case RantObjectType.List:
                    {
                        var sb = new StringBuilder();
                        bool first = true;
                        sb.Append("(");
                        foreach (var rantObject in _list)
                        {
                            if (first)
                            {
                                first = false;
                                sb.Append(", ");
                            }

                            sb.Append(rantObject);
                        }
                        sb.Append(")");
                        return sb.ToString();
                    }
            }
            return "?";
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
    }
}