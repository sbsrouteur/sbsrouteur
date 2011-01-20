Imports System.Math
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.Net
Imports System.Xml.Serialization

Public Class clsSailManager
    Private Const MaxWindSpeedHundedth As Integer = 6000

    Public Enum EnumSail As Integer
        OneSail = 0
        JIB = 1
        SPI = 2
        JIB2 = 4
        GENOA = 8
        CODE_0 = 16
        LIGHT_SPI = 32
        HEAVY_SPY = 64
    End Enum

    Private Const NB_SAILS As Integer = 7

    'Public Shared Sails() As EnumSail = New EnumSail() {EnumSail.CODE_0, EnumSail.GENOA, EnumSail.HEAVY_SPY, EnumSail.JIB, EnumSail.JIB2, EnumSail.LIGHT_SPI, EnumSail.SPI}
    'Public Shared Sails() As EnumSail = New EnumSail() {EnumSail.JIB, EnumSail.SPI}
    Public Shared Sails() As EnumSail = New EnumSail() {EnumSail.OneSail}

    'Dim SailComparer As New SailAngleKey
    Private _SailPolars(6, 180)() As Double
    Private _SailLoaded As Boolean = False
    Private _WindLut(70) As Integer

    Private _PolarLut(,,) As Double
    Private _WindList()() As Integer
    Private _TWAList()() As Integer
    Private _NbWinds() As Integer
    Private _NbAngles As Integer

    Private _Polar(1800, MaxWindSpeedHundedth) As UInt16
    Private _PolarCorner(MaxWindSpeedHundedth, 2) As Double

    Public Const CORNER_SPEED As Integer = 0
    Public Const CORNER_UPWIND As Integer = 1
    Public Const CORNER_DOWNWIND As Integer = 2



    Private Function GetSailIndex(ByVal SailMode As EnumSail) As Integer

        'If SailMode = EnumSail.GENOA Then
        '    Return 3
        'ElseIf SailMode > EnumSail.GENOA Then
        '    'Upper 3
        '    If SailMode = EnumSail.LIGHT_SPI Then
        '        Return 5
        '    ElseIf SailMode = EnumSail.HEAVY_SPY Then
        '        Return 6
        '    Else
        '        Return 4
        '    End If
        'Else
        'Lower 3
        If SailMode = EnumSail.JIB Then
            Return 1
        ElseIf SailMode = EnumSail.SPI Then
            Return 0
        Else
            Return 2
        End If

        'End If
    End Function


    Private Function GetArrayIndex(ByVal A() As Integer, ByVal Value As Double, ByVal ValueBelow As Boolean) As Integer

        Dim RetIndex As Integer = 0
        Dim CurIndex As Integer = 0

        While CurIndex < A.Length

            If ValueBelow And A(CurIndex) > Value Then
                If CurIndex = 0 Then
                    Return 0
                End If
                Return CurIndex - 1
            ElseIf Not ValueBelow And A(CurIndex) >= Value Then
                Return CurIndex
            End If

            CurIndex += 1
        End While

        Return RetIndex
    End Function


    Public Function GetBestSailSpeed(ByVal BoatType As String, ByRef RetSail As EnumSail, ByVal windangle As Double, ByVal windspeed As Double) As Double

        Dim CurS As Double = 0
        Dim RetSpeed As Double = 0
        For Each S In clsSailManager.Sails

            If windspeed < 0 Then
                Throw New NotSupportedException("Negative wind!!")
            End If

            'If (S And BrokenSails) = 0 Then
            CurS = GetSpeed(BoatType, S, windangle, windspeed)
            If CurS > RetSpeed Then
                RetSpeed = CurS
                RetSail = S
            End If
            'End If
        Next

        Return RetSpeed


    End Function

    Public Sub GetCornerAngles(ByVal WindStrength As Double, ByRef MinAngle As Double, ByRef MaxAngle As Double)

        If _PolarCorner(6000, CORNER_SPEED) <> 0 Then
            Dim WindStrengthIndex As Integer = CInt(10 * Math.Round(WindStrength, 2))
            MinAngle = _PolarCorner(WindStrengthIndex, CORNER_UPWIND)
            MaxAngle = _PolarCorner(WindStrengthIndex, CORNER_DOWNWIND)
        Else
            MinAngle = 0
            MaxAngle = 180
        End If
    End Sub

    Public Function GetSpeed(ByVal BoatType As String, ByVal SailMode As EnumSail, ByVal WindAngle As Double, ByVal WindSpeed As Double) As Double

        Dim D As Integer = CInt(10 * ((WindAngle + 360) Mod 180))
        Dim F As Integer = CInt(100 * WindSpeed)

#Const POLAR_STAT = 0
#If POLAR_STAT = 1 Then
        Static NbCall As Long = 0
        Static NbCallCached As Long = 0
        Dim CacheRatio As Double = 0

        NbCall += 1
#End If
        If SailMode = EnumSail.OneSail Then

            If _Polar(D, F) <> 65535 AndAlso _Polar(D, F) <> 0 Then

#If POLAR_STAT = 1 Then
                NbCallCached += 1
                Stats.SetStatValue(Stats.StatID.Polar_CacheRatio) = NbCallCached / NbCall
#End If
                Return _Polar(D, F) / 1000
            End If
        End If

        Dim SailIndex = GetSailIndex(SailMode)
        Dim CurSpeed As Integer = 0


        'WindSpeed = Math.Floor(WindSpeed)
        'WindAngle = Math.Floor(WindAngle + 0.5)

        'If (SailMode And brokensails) <> 0 Then
        '    Return 0
        'End If

        If Not _SailLoaded And Not BoatType Is Nothing Then
            If Not LoadSails(BoatType) Then
                Return -1
            End If
            _SailLoaded = True
        ElseIf BoatType Is Nothing Then
            Return -1
        End If

        Dim WMin As Integer = GetArrayIndex(_WindList(SailIndex), WindSpeed, True)
        Dim WMax As Integer = GetArrayIndex(_WindList(SailIndex), WindSpeed, False)
        Dim AMin As Integer = GetArrayIndex(_TWAList(SailIndex), WindAngle, True)
        Dim AMax As Integer = GetArrayIndex(_TWAList(SailIndex), WindAngle, False)
        'Dim amax As Integer = AMin

        If WMin = WMax AndAlso WMin = 0 Then
            WMax = 1
        End If

        Dim W1 As Integer = _WindList(SailIndex)(WMin)
        Dim W2 As Integer = _WindList(SailIndex)(WMax)
        Dim A1 As Integer = _TWAList(SailIndex)(AMin)
        Dim A2 As Integer = _TWAList(SailIndex)(AMax)


        Dim V11 As Double = _PolarLut(SailIndex, WMin, AMin)
        Dim V21 As Double = _PolarLut(SailIndex, WMax, AMin)
        Dim V12 As Double = _PolarLut(SailIndex, WMin, AMax)
        Dim V22 As Double = _PolarLut(SailIndex, WMax, AMax)
        Dim V1 As Double
        Dim v2 As Double


        If A1 <> A2 Then
            V1 = V11 + (WindAngle - A1) / (A2 - A1) * (V12 - V11)
            v2 = V21 + (WindAngle - A1) / (A2 - A1) * (V22 - V21)
        Else
            V1 = V11
            v2 = V21
        End If

        Dim RetVal As Double
        If W1 <> W2 AndAlso v2 - V1 <> 0 Then
            RetVal = V1 + (WindSpeed - W1) / (W2 - W1) * (v2 - V1)
        Else
            RetVal = V1
        End If

        _Polar(D, F) = CUShort(RetVal * 1000)
#If POLAR_STAT = 1 Then
        Stats.SetStatValue(Stats.StatID.Polar_CacheRatio) = NbCallCached / NbCall
#End If

        Return RetVal

    End Function

    Public Sub InitPolar()

        Dim th As New System.Threading.Thread(AddressOf InitPolarThread)
        th.Start()
    End Sub

    Private Sub InitPolarThread(ByVal StartInfo As Object)

        While Not _SailLoaded
            System.Threading.Thread.Sleep(100)
        End While

        Dim start As DateTime = Now
        For i = 0 To MaxWindSpeedHundedth

            Dim BestUpWind As Double = 0
            Dim BestDownWind As Double = 0

            For alpha = 0 To 1800

                Dim S As Double = GetSpeed("", EnumSail.OneSail, alpha / 10, i / 100)

                If S > _PolarCorner(i, CORNER_SPEED) Then
                    _PolarCorner(i, 0) = S
                End If

                If alpha < 900 AndAlso S * Cos(alpha / 1800 * PI) > BestUpWind Then
                    _PolarCorner(i, CORNER_UPWIND) = alpha / 10 - 2
                    BestUpWind = S * Cos(alpha / 1800 * PI)
                ElseIf alpha >= 900 AndAlso -S * Cos(alpha / 1800 * PI) > BestDownWind Then
                    _PolarCorner(i, CORNER_DOWNWIND) = alpha / 10 + 2
                    BestDownWind = -S * Cos(alpha / 1800 * PI)
                End If

            Next

        Next

        Console.WriteLine("PolarCorner completed " & Now.Subtract(start).ToString)
    End Sub
    Private Function LoadSails(ByVal BoatType As String) As Boolean

        Try
            Dim _WebClient As New WebClient
            Dim sail As EnumSail
            Dim ResponseString As String
            Dim errorcount As Integer = 0
            Dim SailIndex As Integer

            If BoatType Is Nothing OrElse BoatType.Trim = "" Then
                Return False
            End If


            For Each sail In clsSailManager.Sails
                SailIndex = GetSailIndex(sail)
                'Dim s As New System.IO.StreamReader(_WebClient.OpenRead(SailRequestString(sail)).re)
                ResponseString = _WebClient.DownloadString(SailRequestString(sail, BoatType))
                'ResponseString = s.ReadToEnd

                ParseSailString(SailIndex, ResponseString)

            Next

            Return True
        Catch ex As Exception
            'MessageBox.Show("LoadSails Error " & ex.Message)
        End Try
        Return False

    End Function

    Private Sub ParseSailString(ByVal SailIndex As Integer, ByVal S As String)

        Dim Lines() As String = S.Split(CChar(vbLf))
        Dim Line As String
        Dim winds() As String
        Dim i As Integer
        Dim polars() As String
        Dim windangleindex As Integer = 0
        Dim bWindRead As Boolean = False

        For Each Line In Lines
            If Line.Contains("<?xml") OrElse Line.Trim = "" Then
                'nothing
            ElseIf Not bWindRead Then
                'WindLine
                bWindRead = True
                winds = Line.Replace("TWA", "").Split(";"c)
                _NbWinds(SailIndex) = winds.GetUpperBound(0)

                ReDim _WindList(SailIndex)(_NbWinds(SailIndex) - 1)
                For i = 1 To _NbWinds(SailIndex)
                    _WindList(SailIndex)(i - 1) = CInt(winds(i))
                Next

                _NbAngles = Lines.Length - 2
                ReDim _TWAList(SailIndex)(_NbAngles)
                If _PolarLut Is Nothing Then
                    ReDim _PolarLut(6, _NbWinds(SailIndex), _NbAngles)
                End If
            Else
                'polar line
                polars = Line.Replace("</vpp>", "").Split(";"c)
                _TWAList(SailIndex)(windangleindex) = CInt(polars(0))
                For i = 1 To _NbWinds(SailIndex)

                    Double.TryParse(polars(i), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, _
                                     _PolarLut(SailIndex, i - 1, windangleindex))

                Next
                windangleindex += 1
            End If
        Next



    End Sub

    Private Sub newParseSailString(ByVal SailIndex As Integer, ByVal S As String)

        Dim Lines() As String = S.Split(CChar(vbLf))
        Dim Line As String
        Dim Winds() As String = Nothing
        Dim Polars() As String
        Dim i As Integer
        Dim NbWinds As Integer
        Dim WindAngle As Integer
        Dim LastAngleLine As Integer = 0
        Dim j As Integer
        Dim V1 As Double
        Dim V2 As Double
        Dim SmallWindSpeed As Boolean
        Dim Polar_I As Double
        Dim k As Integer

        For Each Line In Lines
            If Line.Contains("<?xml") OrElse Line.Trim = "" Then
                'nothing
            ElseIf Line.StartsWith("<vpp>") Then
                'WindLine
                Winds = Line.Replace("<vpp>TWA", "").Split(";"c)
                NbWinds = Winds.GetUpperBound(0)
            Else
                'polar line
                Polars = Line.Replace("</vpp>", "").Split(";"c)

                WindAngle = CInt(Polars(0))
                SmallWindSpeed = True
                For i = 1 To NbWinds - 1

                    Polar_I = Val(Polars(i))
                    _SailPolars(SailIndex, WindAngle)(CInt(Winds(i))) = Val(Polars(i))
                    _SailPolars(SailIndex, WindAngle)(CInt(Winds(i + 1))) = Val(Polars(i + 1))


                    V1 = _SailPolars(SailIndex, LastAngleLine)(CInt(Winds(i)))
                    V2 = _SailPolars(SailIndex, WindAngle)(CInt(Winds(i + 1)))
                    For k = CInt(Winds(i)) + 1 To CInt(Winds(i + 1)) - 1
                        For j = LastAngleLine + 1 To WindAngle - 1

                            _SailPolars(SailIndex, j)(k) = V1 + (k - CDbl(Winds(i))) * (V2 - V1) / (CDbl(Winds(i + 1)) - CDbl(Winds(i)))

                        Next
                    Next

                Next
                LastAngleLine = WindAngle
            End If
        Next

    End Sub


    Private Sub OrigParseSailString(ByVal SailIndex As Integer, ByVal S As String)

        Dim Lines() As String = S.Split(CChar(vbLf))
        Dim Line As String
        Dim Winds() As String = Nothing
        Dim Polars() As String
        Dim i As Integer
        Dim NbWinds As Integer
        Dim WindAngle As Integer
        Dim LastAngleLine As Integer = 0
        Dim j As Integer
        Dim A2 As Double
        Dim V1 As Double
        Dim V2 As Double
        Dim SmallWindSpeed As Boolean
        Dim Polar_I As Double
        Dim k As Integer

        For Each Line In Lines
            If Line.Contains("<?xml") OrElse Line.Trim = "" Then
                'nothing
            ElseIf Line.StartsWith("<vpp>") Then
                'WindLine
                Winds = Line.Replace("<vpp>TWA", "").Split(";"c)
                NbWinds = Winds.GetUpperBound(0)
            Else
                'polar line
                Polars = Line.Replace("</vpp>", "").Split(";"c)

                WindAngle = CInt(Polars(0))
                SmallWindSpeed = True
                For i = 1 To NbWinds

                    Polar_I = Val(Polars(i))
                    _SailPolars(SailIndex, WindAngle)(CInt(Winds(i))) = Polar_I
                    For k = CInt(Winds(i)) + 1 To 70
                        _SailPolars(SailIndex, WindAngle)(k) = Polar_I
                    Next

                    If Polar_I <> 0 And SmallWindSpeed Then

                        V1 = _SailPolars(SailIndex, LastAngleLine)(CInt(Winds(i)))
                        For j = LastAngleLine To WindAngle - 1
                            A2 = WindAngle
                            V2 = Polar_I
                            For k = 0 To CInt(Val(Winds(i)))
                                _SailPolars(SailIndex, j)(k) = (j - LastAngleLine) / (A2 - LastAngleLine) * (V2 - V1) + V1 'Polar_I 'k / Val(Winds(i)) * Polar_I
                            Next

                            SmallWindSpeed = False
                        Next
                    End If


                    V1 = _SailPolars(SailIndex, LastAngleLine)(CInt(Winds(i)))
                    For j = LastAngleLine + 1 To WindAngle - 1

                        A2 = WindAngle
                        V2 = Polar_I

                        For k = CInt(Winds(i)) To 70
                            _SailPolars(SailIndex, j)(k) = (j - LastAngleLine) / (A2 - LastAngleLine) * (V2 - V1) + V1
                        Next

                    Next
                Next
                LastAngleLine = WindAngle
            End If
        Next

    End Sub

    Private Function SailRequestString(ByVal Sail As EnumSail, ByVal BoatType As String) As String
        'Dim s As String = RouteurModel.BASE_GAME_URL & "/resources/vpp/vpp_1_" & Sail & ".xml?ver=17_118"
        Dim s As String = RouteurModel.BASE_SAIL_URL & "/" & BoatType & ".csv"
        Return s
    End Function


    Public Sub New()

        Dim i As Integer
        Dim j As Integer

        For i = 0 To 6
            For j = 0 To 180
                ReDim _SailPolars(i, j)(70) 'New Dictionary(Of Integer, Double)
            Next

        Next

        For i = 0 To 70
            _WindLut(i) = -1
        Next

        ReDim _NbWinds(NB_SAILS - 1)
        ReDim _TWAList(NB_SAILS - 1)
        ReDim _WindList(NB_SAILS - 1)

        For i = 0 To 1800
            For j = 0 To 6000
                _Polar(i, j) = 65535
            Next
        Next
    End Sub

End Class

Public Class SailConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        Return CType(value, clsSailManager.EnumSail).ToString

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class