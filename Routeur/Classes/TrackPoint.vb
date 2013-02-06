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

Public Class TrackPoint

    Implements INotifyPropertyChanged


    Private _P As Coords
    Private _Epoch As Long

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Property Epoch As Long
        Get
            Return _Epoch
        End Get
        Set(value As Long)
            _Epoch = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Epoch"))
        End Set
    End Property

    Public Property P As Coords
        Get
            Return _P
        End Get
        Set(value As Coords)
            _P = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("P"))
        End Set
    End Property


End Class
