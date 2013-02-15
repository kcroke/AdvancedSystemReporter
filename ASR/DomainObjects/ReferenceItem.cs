﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data.Items;

namespace ASR.DomainObjects
{
	
	public class ReferenceItem : CustomItem
	{
        private string _currentuser;
	    public ReferenceItem(Item i) : base(i)
	    {
	        _currentuser = Sitecore.Context.User.Name;
	    }


        #region ItemFields
        public string Assembly
        {
            get { return InnerItem["assembly"]; }
        }

        public string Class
        {
            get { return InnerItem["class"]; }
        }

        public string Attributes
        {
            get { return InnerItem["attributes"]; }
        } 
        #endregion

        public string ReplacedAttributes
		{
			get
			{
				string _replacedAttributes = Attributes;
				foreach (var pi in Parameters)
				{
					string tag = string.Concat('{', pi.Name, '}');
					_replacedAttributes = _replacedAttributes.Replace(tag, pi.Value);
				}

                if (_replacedAttributes.Contains("$"))
                {
                    _replacedAttributes = Util.MakeDateReplacements(_replacedAttributes);
                    _replacedAttributes = _replacedAttributes.Replace("$sc_currentuser", _currentuser); 
                }
			    return _replacedAttributes;
			}
		}
		public string FullType
		{
			get
			{
				if (!string.IsNullOrEmpty(Assembly))
				{
					return string.Concat(Class, ", ", Assembly);
				}
				return Class;
			}
		}

		public void SetAttributeValue(string tag, string value)
		{
			try
			{

				ParameterItem pi = Parameters.First(p => p.Name == tag);

				if (pi != null)
				{
					pi.Value = Uri.UnescapeDataString(value);
				}
			}
			// can't find element
			catch (InvalidOperationException)
			{
				// do nothing;
			}
		}

		public bool HasParameters
		{
			get
			{
				if (_parameters == null)
				{
					makeParameterSet();
				}

				return _parameters.Count > 0;
			}
		}

		private HashSet<ParameterItem> _parameters = null;
		public IEnumerable<ParameterItem> Parameters
		{
			get
			{
				if (_parameters == null || _parameters.Count == 0)
				{
					makeParameterSet();
				}
				return _parameters;
			}
		}

	    public string PrettyName
	    {
	        get { return string.Format("{0} ({1})", Name,Name); }	        
	    }

	    private void makeParameterSet()
		{
			_parameters = new HashSet<ParameterItem>();
			foreach (var tag in extractParameters(Attributes))
			{
				ParameterItem pi = findItem(tag);
				if (pi != null)
				{
					_parameters.Add(pi);
				}
			}
		}

		private ParameterItem findItem(string name)
		{
			string path = string.Concat(Settings.Instance.ParametersFolder, "/", name);
            
			return new ParameterItem(this.Database.GetItem(path));
		}

		private IEnumerable<string> extractParameters(string st)
		{
			List<string> tags = new List<string>();
			Regex r = new Regex(@"\{(\w*)\}");
			Match m = r.Match(st);

			while (m.Success)
			{
				if (m.Groups.Count == 2)
				{
					tags.Add(m.Groups[1].Value);
				}
				m = m.NextMatch();
			}
			return tags;
		}
	}
}
