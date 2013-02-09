'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

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

Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRouterPrefs

    Private _Context As RacePrefs
    Private _ID As String

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.

    End Sub

    Public Sub New(ByVal ID As String)

        Me.New()
        _ID = ID
        _Context = RacePrefs.GetRaceInfo(_ID)
        DataContext = _Context

    End Sub

    Private Sub cmdOKClick(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        RacePrefs.Save(_ID, _Context)
        Hide()
    End Sub

    Private Sub cmdCancelClick(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        _Context = RacePrefs.GetRaceInfo(_ID)
        Hide()
    End Sub

End Class
