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

Public Class VLMShortRaceInfo

    Public Property idraces As String
    Public Property racename As String
    Public Property started As Integer
    Public Property deptime As Long
    Public Property startlong As Double
    Public Property startlat As Double
    Public Property boattype As String
    Public Property closetime As Long
    Public Property racetype As Long
    Public Property firstpcttime As Double
    Public Property depend_on As String
    Public Property qualifying_races As String
    Public Property idchallenge As String
    Public Property coastpenalty As Long
    Public Property bobegin As Long
    Public Property boend As Long
    Public Property maxboats As Long
    Public Property theme As String
    Public Property vacfreq As Long
    Public Property updated As String

    Public ReadOnly Property IsStarted As Boolean
        Get
            Return started = 1
        End Get
    End Property

    Public ReadOnly Property CanEngage As Boolean
        Get
            Dim StartDate As DateTime
            Dim EndDate As DateTime

            Dim Cvt As New EpochToUTCDateConverter

            StartDate = CDate(Cvt.Convert(deptime, GetType(DateTime), Nothing, System.Globalization.CultureInfo.CurrentCulture)).ToLocalTime
            EndDate = CDate(Cvt.Convert(closetime, GetType(DateTime), Nothing, System.Globalization.CultureInfo.CurrentCulture)).ToLocalTime

            Return Now <= StartDate Or Now < EndDate


        End Get
    End Property

End Class
