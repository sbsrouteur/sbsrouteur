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

Imports System.Math

Public Class GridProgressContext
    Inherits ProgressContext

    Private _StartDist As Double = -1
    Private _StartTime As DateTime
    Private _EstCount As Long

    Public Sub New()

        Title = "Grid Progress"

    End Sub

    Public Sub Start(ByVal Dist As Double, ByVal k As Double)

        _StartDist = Dist
        _StartTime = Now
        '
        ' Greeting to |AZRAEL| for this one ;)
        '
        '
        ' Ellipsis area :  A = pi * sqrt([0.5 * d_F]^2 + [0.5 * c * d_F] ^2) * 0.5 * c * d_F
        ' c=k
        ''
        _EstCount = CLng(PI * Sqrt((Dist / 2) ^ 2 + (k * Dist / 2) ^ 2) * k * Dist / 2 / (RouteurModel.GridGrain ^ 2) / 10000)

    End Sub

    Public Sub Progress(ByVal GridCount As Long, ByVal Pending As Long, ByVal Dist As Double)

        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        ProgressValue = (_StartDist - Dist) / _StartDist * 100
        If Dist <> 0 Then
            ProgressETA = TimeSpan.FromSeconds((_EstCount - GridCount) / GridCount * TotalSeconds)
        Else
            ProgressETA = Now.Subtract(Now)

        End If
    End Sub


End Class
