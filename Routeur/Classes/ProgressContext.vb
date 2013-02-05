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

Public MustInherit Class ProgressContext

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event RequestVisibility(ByVal Vis As Visibility)

    Private _ProgressValue As Double
    Private _ProgressETA As TimeSpan
    Private _Dirty As Boolean = False

    Private _Title As String

    Public Property Dirty() As Boolean
        Get
            Return _Dirty
        End Get
        Set(ByVal value As Boolean)
            _Dirty = value
        End Set
    End Property

    Public Property ProgressETA() As TimeSpan
        Get
            Return _ProgressETA
        End Get
        Set(ByVal value As TimeSpan)

            If _ProgressETA <> value Then
                _ProgressETA = value
                Dirty = True
            End If
        End Set
    End Property

    Public Property ProgressValue() As Double
        Get
            Return _ProgressValue
        End Get
        Set(ByVal value As Double)

            If value <> _ProgressValue Then
                _ProgressValue = value
                Dirty = True
            End If

        End Set
    End Property

    Public Overridable Sub OnPropertyChanged(ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, e)

    End Sub

    Public Overridable Sub OnRequestVisibility(ByVal Vis As Visibility)
        RaiseEvent RequestVisibility(Vis)
    End Sub

    Public Sub refresh()
        Dirty = False
        OnPropertyChanged(New PropertyChangedEventArgs("ProgressValue"))
        OnPropertyChanged(New PropertyChangedEventArgs("ProgressETA"))

    End Sub

    Public Property Title() As String
        Get
            Return _Title
        End Get
        Set(ByVal value As String)

            If Title <> value Then
                _Title = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Title"))
            End If
        End Set
    End Property


End Class


