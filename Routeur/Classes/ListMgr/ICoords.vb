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

Public Interface ICoords

    Property Lon() As Double
    Property Lat() As Double
    ReadOnly Property N_Lon() As Double
    ReadOnly Property N_Lat() As Double

    Function Equals(ByVal C As ICoords) As Boolean
    Function GetHashCode() As Long

End Interface
