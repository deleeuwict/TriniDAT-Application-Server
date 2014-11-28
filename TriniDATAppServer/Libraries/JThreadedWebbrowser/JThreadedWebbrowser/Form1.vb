Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        'inject JQuery
        'HtmlElement head = webBrowser1.Document.GetElementsByTagName("head")[0];
        'HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
        'IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
        'element.text = "function sayHello() { alert('hello') }";
        'head.AppendChild(scriptEl);
        'webBrowser1.Document.InvokeScript("sayHello");


    End Sub
End Class
