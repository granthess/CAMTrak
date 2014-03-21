using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace QuickStart.Utils
{
    public class TypeReflectionInfo
    {
        public String typeName;
        public PropertyDetails properties;
        public List<MethodInfo> methods;        

        public TypeReflectionInfo( Object obj, Type type )
        {            
            this.typeName = obj.GetType().ToString();            
            this.methods = new List<MethodInfo>();
            
            this.properties = new PropertyDetails();
            this.properties.children = new List<PropertyDetails>();
            this.properties.name = obj.ToString();
            this.properties.readOnly = type.IsValueType;
            this.properties.type = type;
            this.properties.value = obj;
           
            QSUtils.DepthFirstPropertyDetails(this.properties);            
        }

        public void AddMethods( Type type )
        {
            MethodInfo[] methods = type.GetMethods();
            
            // We filter for methods that are written into the QuickStart Engine,
            // we don't need things like 'GetHashCode', which exists on all types
            methods.Where(p => IsQSMethod(p.Name));
            
            this.methods = methods.ToList();
        }

        private bool IsQSMethod( String name )
        {
            switch (name)
            {
                case "ToString":
                case "Equals":
                case "GetHashCode":
                case "GetType":
                    return false;
                default:
                    return true;
            }
        }
    }

    public class PropertyDetails
    {
        public String name;
        public Object value;
        public Type type;
        public Type parentType;
        public bool readOnly;
        public List<PropertyDetails> children;

        public PropertyDetails()
        {
        }
        
        public PropertyDetails( Object obj, PropertyInfo info, Type parentType )
        {
            this.name = info.Name;
            this.readOnly = !info.CanWrite;
            this.value = info.CanRead ? info.GetValue(obj, null) : null;
            this.type = info.PropertyType;
            this.parentType = parentType;
            this.children = new List<PropertyDetails>();           
        }

        public Type GetBaseType()
        {
            if (null == type)
                return null;

            return type.BaseType;
        }
    }

    public static class QSUtils
    {
        /// <summary>
        /// Gives a string output of the reflection info on this type
        /// </summary>
        /// <param name="type">The Type to be reflected</param>
        /// <param name="output">String output of reflection information</param>
        /// <returns>Returns 'type's BaseType, which is 'null' if there is no BaseType</returns>
        static public TypeReflectionInfo GetReflectionInfo( Object obj, Type type )
        {
            if (null == type)
                return null;
            
            TypeReflectionInfo info = new TypeReflectionInfo(obj, type);            
            //info.AddMethods(type);

            return info;
        }

        static public void DepthFirstPropertyDetails( PropertyDetails parent, int currentDepth = 0, int maxDepth = 3 )
        {
            PropertyInfo[] props = parent.type.GetProperties(BindingFlags.Public
                                                           | BindingFlags.Instance);        

            foreach (PropertyInfo propertyInfo in props)
            {
                if ( propertyInfo.PropertyType.IsPrimitive )
                    continue;

                PropertyDetails newDetails = new PropertyDetails(parent.value, propertyInfo, parent.type);

                if (null != newDetails.value)
                {
                    // If we have a base type we depth-first add its details to our own
                    if (currentDepth < maxDepth && null != newDetails.type.Name
                        && newDetails.type.Name != "Object"
                        && !newDetails.type.IsGenericType)
                    {
                        DepthFirstPropertyDetails(newDetails, ++currentDepth);
                    }

                    parent.children.Add(newDetails);
                }
            }

            --currentDepth;
        }

        static public List<string> ConvertTypeReflectionInfoToString( PropertyDetails parentDetails, int indentLevel = 0 )
        {           
            List<string> output = new List<string>();

            for (int j = 0; j < parentDetails.children.Count; ++j)
            {
                PropertyDetails details = parentDetails.children[j];

                String item = string.Format("{0} => {1}", details.name, details.value);
                item = item.PadLeft(indentLevel);
                output.Add(item);

                indentLevel += 2;
                List<string> childOutput = QSUtils.ConvertTypeReflectionInfoToString(details, indentLevel);
                output.AddRange(childOutput);
                indentLevel -= 2;
            }

            return output;
        }        
    }
}
