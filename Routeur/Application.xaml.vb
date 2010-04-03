Imports System.Windows.Markup
Imports System.Globalization

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Public Sub New()
        FrameworkElement.LanguageProperty.OverrideMetadata( _
                  GetType(FrameworkElement), _
                  New FrameworkPropertyMetadata( _
                     XmlLanguage.GetLanguage( _
                     CultureInfo.CurrentCulture.IetfLanguageTag)))
    End Sub
End Class
