#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ParadoxNotion.Design
{

    ///<summary>Documentation from XML</summary>
    public static class XMLDocs
    {
        private static Dictionary<MemberInfo, XmlElement> cachedElements = new Dictionary<MemberInfo, XmlElement>();
        private static Dictionary<MemberInfo, string> cachedSummaries = new Dictionary<MemberInfo, string>();

        ///<summary>Returns a chached summary info for member</summary>
        public static string GetMemberSummary(MemberInfo memberInfo) {
            string result;
            if ( cachedSummaries.TryGetValue(memberInfo, out result) ) { return result; }
            var element = GetXmlElementForMember(memberInfo);
            return cachedSummaries[memberInfo] = ( element != null ? element["summary"].InnerText.Trim() : "No Documentation Found" );
        }

        ///<summary>Returns a chached return info for method</summary>
        public static string GetMethodReturn(MethodBase method) {
            var element = GetXmlElementForMember(method);
            return element != null ? element["returns"].InnerText.Trim() : null;
        }

        ///<summary>Returns a semi-chached method parameter info for method</summary>
        public static string GetMethodParameter(MethodBase method, ParameterInfo parameter) { return GetMethodParameter(method, parameter.Name); }

        ///<summary>Returns a semi-chached method parameter info for method</summary>
        public static string GetMethodParameter(MethodBase method, string parameterName) {
            var methodElement = GetXmlElementForMember(method);
            if ( methodElement != null ) {
                foreach ( var element in methodElement ) {
                    var xmlElement = element as XmlElement;
                    if ( xmlElement == null ) { continue; }
                    var found = xmlElement.Attributes["name"];
                    if ( found != null && found.Value == parameterName ) {
                        return xmlElement.InnerText.Trim();
                    }
                }
            }
            return null;
        }

        ///<summary>Returns a cached XML elements for member</summary>
        static XmlElement GetXmlElementForMember(MemberInfo memberInfo) {

            if ( memberInfo is MethodInfo ) {
                var method = (MethodInfo)memberInfo;
                if ( method.IsPropertyAccessor() ) { memberInfo = method.GetAccessorProperty(); }
            }

            if ( memberInfo == null ) { return null; }

            XmlElement element;
            if ( cachedElements.TryGetValue(memberInfo, out element) ) { return element; }

            if ( memberInfo is MethodInfo ) {
                element = GetMemberDoc((MethodInfo)memberInfo);
            } else if ( memberInfo is Type ) {
                element = GetTypeDoc((Type)memberInfo);
            } else {
                element = GetMemberDoc(memberInfo);
            }

            return cachedElements[memberInfo] = element;
        }

        static XmlElement GetTypeDoc(Type type) { return GetDoc(type, $"T:{type.FullName}"); }

        static XmlElement GetMemberDoc(MemberInfo memberInfo) {
            if ( memberInfo is MethodInfo ) {
                var methodInfo = (MethodInfo)memberInfo;
                var parameters = methodInfo.GetParameters();
                if ( parameters.Length == 0 ) {
                    return GetDoc(methodInfo.DeclaringType, $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}");
                }
                var parametersString = string.Empty;
                for ( var i = 0; i < parameters.Length; i++ ) {
                    parametersString += parameters[i].ParameterType.FullName + ( i < parameters.Length - 1 ? "," : string.Empty );
                }
                return GetDoc(methodInfo.DeclaringType, $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}({parametersString})");
            }
            return GetDoc(memberInfo.DeclaringType, $"{memberInfo.MemberType.ToString()[0]}:{memberInfo.DeclaringType.FullName}.{memberInfo.Name}");
        }

        static XmlElement GetDoc(Type type, string pathName) {
            var xmlPath = Path.ChangeExtension(type.Assembly.CodeBase.Substring(8), ".xml");
            if ( !File.Exists(xmlPath) ) { return null; }
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            return xmlDocument.SelectSingleNode("//member[starts-with(@name, '" + pathName + "')]") as XmlElement;
        }
    }
}

#endif