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

        Dim currentDomain As AppDomain = AppDomain.CurrentDomain
        AddHandler currentDomain.UnhandledException, AddressOf CrashCatcher
    End Sub

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        MessageBox.Show("App unhandled exception :" & e.Exception.Message & vbCrLf & e.Exception.StackTrace)
    End Sub

    Private Sub CrashCatcher(sender As Object, ev As UnhandledExceptionEventArgs)
        Dim e As Exception = CType(ev.ExceptionObject, Exception)
        MessageBox.Show("App unhandled exception :" & e.Message & vbCrLf & e.StackTrace)
    End Sub

End Class
