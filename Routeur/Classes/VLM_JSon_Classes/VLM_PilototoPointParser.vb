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

Imports Routeur.RoutePointView

Module VLM_PilototoPointParser

    Public Const FLD_ID As Integer = 0
    Public Const FLD_DATEVALUE As Integer = 1
    Public Const FLD_ORDERTYPE As Integer = 2
    Public Const FLD_ORDERVALUE As Integer = 3
    Public Const FLD_LATVALUE As Integer = 3
    Public Const FLD_LONVALUE As Integer = 4

    Public Const MIN_PILOTOTO_FIED_COUNT As Integer = 5



    Public Function GetBearingAngleValue(ByVal OrderValue As String, ByRef RetValue As Double) As Boolean
        Return Double.TryParse(OrderValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetValue)
    End Function

    Public Function GetOrderDate(ByVal Datestring As String, ByRef RetValue As DateTime) As Boolean
        Dim LongValue As Long
        If Long.TryParse(Datestring, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, LongValue) Then
            RetValue = New Date(1970, 1, 1).AddSeconds(LongValue).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function GetOrderID(ByVal OrderString As String, ByRef RetID As Integer) As Boolean

        Dim IntValue As Integer
        If Integer.TryParse(OrderString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, IntValue) Then
            RetID = CType(IntValue, EnumRouteMode)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function GetOrderType(ByVal OrderString As String, ByRef RetValue As EnumRouteMode) As Boolean

        Dim IntValue As Integer
        If Integer.TryParse(OrderString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, IntValue) Then
            RetValue = CType(IntValue, EnumRouteMode)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function GetWPInfo(ByVal LatString As String, ByVal LonString As String, ByRef Lon As Double, ByRef Lat As Double, ByRef WithBearing As Boolean, ByRef Bearing As Double) As Boolean

        Double.TryParse(LatString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat)
        If LonString.Contains("@") Then
            Double.TryParse(LonString.Substring(0, LonString.IndexOf("@"c)), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)
            Double.TryParse(LonString.Substring(LonString.IndexOf("@"c) + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Bearing)
            WithBearing = Bearing <> -1
        Else
            Double.TryParse(LonString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)
            WithBearing = False

        End If

        Return True

    End Function

    Public Function OrderIsPending(ByVal OrderString As String) As Boolean

        Return OrderString.ToLowerInvariant.Contains("pending")

    End Function

End Module
