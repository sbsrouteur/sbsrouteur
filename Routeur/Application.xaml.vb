'This file is part of Routeur.
'Copyright (C) 2011  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

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
