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

Imports System.ComponentModel
Imports System.IO

Public Class AppPrefs

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged


    Public Property AutoStartRouterAtMeteoUpdate As Boolean = False

    Public Shared Sub Load(ByVal Prefs As AppPrefs)

        Dim BaseFileName As String = IO.Path.Combine(RouteurModel.BaseFileDir, "AppPrefs.xml")

        If IO.File.Exists(BaseFileName) Then
            Dim SR As New Xml.Serialization.XmlSerializer(GetType(AppPrefs))
            Using Fr As New StreamReader(BaseFileName)
                Prefs = CType(SR.Deserialize(Fr), AppPrefs)
            End Using
        End If

        Return
    End Sub

    Public Sub save()
        Dim BaseFileName As String = IO.Path.Combine(RouteurModel.BaseFileDir, "AppPrefs.xml")

        Dim SR As New Xml.Serialization.XmlSerializer(GetType(AppPrefs))
        Using Fr As New StreamWriter(BaseFileName)
            SR.Serialize(Fr, Me)
        End Using


    End Sub

End Class
