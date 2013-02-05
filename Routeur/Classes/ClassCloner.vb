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

Public Class ClassCloner

    Public Shared Sub CloneObject(ByVal O As FrameworkElementFactory, ByVal Source As DependencyObject, ByVal SourcePath As String, ByVal DestPath As String)

        Dim Count As Integer = VisualTreeHelper.GetChildrenCount(Source)
        Dim i As Integer

        CloneObjectLocalProperties(O, Source, SourcePath, DestPath)

        For i = 0 To Count - 1

            Dim fe As New FrameworkElementFactory(VisualTreeHelper.GetChild(Source, i).GetType)

            CloneObjectLocalProperties(fe, VisualTreeHelper.GetChild(Source, i), SourcePath, DestPath)

            CloneObject(fe, VisualTreeHelper.GetChild(Source, i), SourcePath, DestPath)
            O.AppendChild(fe)

        Next

    End Sub


    Private Shared Sub CloneObjectLocalProperties(ByVal O As FrameworkElementFactory, ByVal Source As DependencyObject, ByVal SourcePath As String, ByVal DestPath As String)
        Dim E = Source.GetLocalValueEnumerator
        While E.MoveNext

            If TypeOf E.Current.Value Is BindingExpression Then
                Dim B As Binding

                B = CType(E.Current.Value, BindingExpression).ParentBinding

                If B.Path.Path = SourcePath Then
                    B = CType(CloneUsingXaml(CType(E.Current.Value, BindingExpression).ParentBinding), Binding)
                    With B
                        .Path.Path = DestPath
                    End With
                End If

                O.SetBinding(E.Current.Property, B)
            Else
                O.SetValue(E.Current.Property, Source.GetValue(E.Current.Property))

            End If
        End While
    End Sub


    Public Shared Function CloneUsingXaml(ByVal o As Object) As Object


        Dim xaml = System.Windows.Markup.XamlWriter.Save(o)

        Return System.Windows.Markup.XamlReader.Load(New System.Xml.XmlTextReader(New System.IO.StringReader(xaml)))

    End Function

End Class
