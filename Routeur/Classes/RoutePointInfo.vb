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

Imports System.ComponentModel

Public Class RoutePointInfo

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _RouteName As String
    Private _P As VLM_Router.clsrouteinfopoints

    Public Sub New()

    End Sub

    Public Sub New(ByVal Name As String, ByVal P As VLM_Router.clsrouteinfopoints)

        _RouteName = Name
        _P = P

    End Sub

    Public Property P() As VLM_Router.clsrouteinfopoints
        Get
            Return _P
        End Get
        Set(ByVal value As VLM_Router.clsrouteinfopoints)
            _P = value
        End Set
    End Property

    Public ReadOnly Property Route() As IList(Of VLM_Router.clsrouteinfopoints)
        Get
            Dim RetCol As New List(Of VLM_Router.clsrouteinfopoints)

            Dim P As VLM_Router.clsrouteinfopoints = _P

            While P IsNot Nothing
                RetCol.Insert(0, P)
                P = P.From
            End While

            Return RetCol
        End Get
    End Property


    Public Property RouteName() As String
        Get
            Return _RouteName
        End Get
        Set(ByVal value As String)
            _RouteName = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
