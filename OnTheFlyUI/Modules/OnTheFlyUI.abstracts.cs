using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenMetaverse;
using OpenMetaverse.StructuredData;

using Aurora.Framework;

namespace Aurora.Addon.OnTheFlyUI
{
    public abstract class CommonHasChildren<T> : List<T>
    {
        public string Type
        {
            get { return "OnTheFlyUI"; }
        }

        protected Attributes m_Attributes;
        public Attributes Attributes
        {
            get { return m_Attributes; }
        }

        protected DataBind m_DataBind;
        public DataBind DataBind
        {
            get { return m_DataBind; }
        }

        public CommonHasChildren()
        {
            m_Attributes = new Attributes(0);
            m_DataBind = new DataBind(0);
        }

        public CommonHasChildren(IEnumerable<T> children)
            : base(children)
        {
            m_Attributes = new Attributes(0);
            m_DataBind = new DataBind(0);
        }

        public CommonHasChildren(Attributes attributes, IEnumerable<T> children)
            : base(children)
        {
            m_Attributes = attributes;
            m_DataBind = new DataBind(0);
        }

        public CommonHasChildren(Attributes attributes, DataBind databind, IEnumerable<T> children)
            : base(children)
        {
            m_Attributes = attributes;
            m_DataBind = databind;
        }

        public CommonHasChildren(Attributes attributes)
        {
            m_Attributes = attributes;
            m_DataBind = new DataBind(0);
        }

        public CommonHasChildren(Attributes attributes, DataBind databind)
        {
            m_Attributes = attributes;
            m_DataBind = databind;
        }

        public OSD ToOSD()
        {
            OSDMap resp = new OSDMap();
            resp["Type"] = new OSDString(Type);
            resp["Attributes"] = Attributes.ToOSD();
            resp["DataBind"] = DataBind.ToOSD();

            OSDArray children = new OSDArray(this.Count);
            foreach (ITableChild child in this)
            {
                children.Add(child.ToOSD());
            }
            resp["Children"] = children;

            return resp;
        }
    }

    public abstract class Container<T> : CommonHasChildren<T>, IContainer
    {
        public new string Type
        {
            get { return base.Type + ".Container"; }
        }

        public Container()
            : base()
        {
        }

        public Container(IEnumerable<T> children)
            : base(children)
        {
        }

        public Container(Attributes attributes, IEnumerable<T> children)
            : base(attributes, children)
        {
        }

        public Container(Attributes attributes, DataBind databind, IEnumerable<T> children)
            : base(attributes, databind, children)
        {
        }

        public Container(Attributes attributes)
            : base(attributes)
        {
        }

        public Container(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
        }
    }

    public abstract class ElementParent<T> : CommonHasChildren<T>, IElement
    {
        public new string Type
        {
            get { return base.Type + ".Element"; }
        }

        public ElementParent()
            : base()
        {
        }

        public ElementParent(IEnumerable<T> children)
            : base(children)
        {
        }

        public ElementParent(Attributes attributes, IEnumerable<T> children)
            : base(attributes, children)
        {
        }

        public ElementParent(Attributes attributes, DataBind databind, IEnumerable<T> children)
            : base(attributes, databind, children)
        {
        }

        public ElementParent(Attributes attributes)
            : base(attributes)
        {
        }

        public ElementParent(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
        }
    }

    public abstract class Element<T> : IElement
    {
        public string Type
        {
            get { return "OnTheFlyUI.Element"; }
        }

        protected Attributes m_Attributes;
        public Attributes Attributes
        {
            get { return m_Attributes; }
        }

        protected DataBind m_DataBind;
        public DataBind DataBind
        {
            get { return m_DataBind; }
        }

        public T Value { get; set; }

        public Element()
        {
            m_DataBind = new DataBind(0);
        }

        public Element(T value)
        {
            Value = value;
            m_DataBind = new DataBind(0);
        }

        public Element(DataBind databind)
        {
            m_DataBind = databind;
        }

        public OSD ToOSD()
        {
            OSDMap resp = new OSDMap();
            resp["Type"] = new OSDString(Type);
            resp["Attributes"] = Attributes.ToOSD();
            resp["DataBind"] = DataBind.ToOSD();
            resp["Value"] = OSD.FromObject(Value);

            return resp;
        }
    }
}
