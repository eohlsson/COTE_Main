using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.UI;

public static partial class HtmlHelpers
   {
       public static void ShowRadioButtonList<T>(this ViewPage page, IList<T> list, string name, Expression<Func<T, object>> valueProperty, Expression<Func<T, object>> displayProperty, System.Web.UI.WebControls.Orientation orientation)
       {
           page.ShowRadioButtonList(list, name, valueProperty, displayProperty, "3", orientation);
       }
    
       public static void ShowRadioButtonList<T>(this ViewPage page, IList<T> list, string name, Expression<Func<T, object>> valueProperty, Expression<Func<T, object>> displayProperty, string selectedValue, System.Web.UI.WebControls.Orientation orientation)
       {
           HtmlTextWriter writer = new HtmlTextWriter(page.Response.Output);
           if (writer != null)
           {
               Func<T, object> valueGetter = valueProperty.Compile();
               Func<T, object> displayStringGetter = displayProperty.Compile();
    
               for (int i = 0; i < list.Count; i++)
               {
                   T row = list[i];
                   string value = valueGetter(row).ToString();
                   writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                   writer.AddAttribute(HtmlTextWriterAttribute.Id, name + "_" + i);
                   writer.AddAttribute(HtmlTextWriterAttribute.Name, name, true);
                   writer.AddAttribute(HtmlTextWriterAttribute.Value, value, true);
                   if (value == selectedValue)
                   {
                       writer.AddAttribute(HtmlTextWriterAttribute.Checked,"checked");
                   }
                   writer.RenderBeginTag(HtmlTextWriterTag.Input);
                   writer.Write(displayStringGetter(row));
                   writer.RenderEndTag();
    
                   if (orientation == System.Web.UI.WebControls.Orientation.Vertical)
                   {
                       writer.RenderBeginTag(HtmlTextWriterTag.Br);
                       writer.RenderEndTag();
                   }
               }
           }
       }
   }