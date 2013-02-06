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

Public Class Digit

    Implements INotifyPropertyChanged

    Private _Dot As Boolean = False
    Private _Digit As Char = "0"c


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Property Digit() As Char
        Get
            Return _Digit
        End Get
        Set(ByVal value As Char)
            _Digit = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Digit"))
        End Set
    End Property

    Public Property Dot() As Boolean
        Get
            Return _Dot
        End Get
        Set(ByVal value As Boolean)
            _Dot = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Dot"))
        End Set
    End Property
End Class
