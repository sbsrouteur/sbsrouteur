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

Public Class MapProgressContext
    Inherits ProgressContext

    Private _NbToProcess As Long
    Private _CurPoint As Long
    Private _StartTime As DateTime

    Public Sub New(ByVal TitleString As String)

        Title = TitleString

    End Sub

    Public Sub Start(ByVal NbToProcess As Long)

        _NbToProcess = NbToProcess
        _CurPoint = 0
        _StartTime = Now
        OnRequestVisibility(Visibility.Visible)
        ProgressValue = 0

    End Sub

    Public Sub Progress(ByVal CurVal As Long)

        If _NbToProcess = 0 Then
            Return
        End If

        Dim TotalSeconds As Double = Now.Subtract(_StartTime).TotalSeconds
        Dim Pct As Double = CurVal / _NbToProcess
        ProgressValue = 100 * Pct
        If Pct <> 0 Then
            ProgressETA = TimeSpan.FromSeconds(TotalSeconds / Pct * (1 - Pct))
        Else
            ProgressETA = Now.Subtract(Now)

        End If
        If ProgressValue = 100 Then
            OnRequestVisibility(Visibility.Hidden)
        End If
    End Sub


End Class
