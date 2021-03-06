﻿'This file is part of Routeur.
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

Public Class MapSegment

    Implements IEquatable(Of MapSegment)

    Public Sub New()

    End Sub

    Sub New(lon1 As Double, lat1 As Double, lon2 As Double, lat2 As Double)
        ' TODO: Complete member initialization 
        _Lon1 = lon1
        _Lat1 = lat1
        _Lon2 = lon2
        _Lat2 = lat2
    End Sub

    Public Property Id As Long
    Public Property Lon1 As Double
    Public Property Lon2 As Double
    Public Property Lat1 As Double
    Public Property Lat2 As Double

    Public Function Equals1(other As MapSegment) As Boolean Implements System.IEquatable(Of MapSegment).Equals
        Return (Lon1 = other.Lon1 And Lat1 = other.Lat1 And Lon2 = other.Lon2 And Lat2 = other.Lat2) OrElse
                (Lon2 = other.Lon1 And Lat2 = other.Lat1 And Lon1 = other.Lon2 And Lat1 = other.Lat2)
    End Function

    Public ReadOnly Property P1 As Coords
        Get
            Return New Coords(Lat1, Lon1)
        End Get
    End Property

    Public ReadOnly Property P2 As Coords
        Get
            Return New Coords(Lat2, Lon2)
        End Get
    End Property
End Class
