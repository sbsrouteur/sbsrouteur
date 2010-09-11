﻿Imports System.ComponentModel
Imports System.Net
Imports System.Threading


Public Class GribManager
    Implements INotifyPropertyChanged

    Private Enum DirInterpolation As Integer

        Pos = 1
        Neg = 0
        UV = -1

    End Enum


    Public Const GRIB_MAX_DAY As Integer = 16

    Private Const MAX_GRIB_05 As Integer = 180
    Private Const MAX_GRIB_1 As Integer = 384
    Private Const GRIB_OFFSET As Integer = -3
    Private Const GRIB_GRAIN_05 As Integer = 3
    Private Const GRIB_GRAIN_1 As Integer = 12
    Private Const GRIB_PERIOD As Integer = 6
    Private Const MAX_INDEX_05 As Integer = CInt(MAX_GRIB_05 / GRIB_GRAIN_05)
    Private Const MAX_INDEX_1 As Integer = CInt((MAX_GRIB_1 - MAX_GRIB_05 - GRIB_GRAIN_1) / GRIB_GRAIN_1) + MAX_INDEX_05
    Private Const MAX_INDEX As Integer = MAX_INDEX_1

    Public Shared ZULU_OFFSET As Integer = -CInt(TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours)

    Private Shared _GMT_Offset As Double = TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours
    Private Shared _GribMonitor As New Object

    Private _MeteoArrays(MAX_INDEX) As MeteoArray
    Private _LastGribDate As DateTime
    Public Event log(ByVal Msg As String)

    Private Const CORRECTION_LENGTH As Integer = 12
    Private _AnglePointer As Integer = 0
    Private _WindPointer As Integer = 0
    Private WithEvents _Process As New Process
    Private _Evt As New AutoResetEvent(False)


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Const SampleGribURL As String = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_hd.pl?file=gfs.t06z.mastergrb2f03&lev_10_m_above_ground=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon=-5&rightlon=5&toplat=60&bottomlat=40&dir=%2Fgfs.2009082006%2Fmaster"

    Private Function CheckGribData(ByVal MeteoIndex As Integer, ByVal LonIndex As Integer, ByVal latIndex As Integer, ByVal NoLock As Boolean) As Boolean

        'Static NextTick As Long = 0
        Dim bLoad As Boolean = False
        Dim retval As Boolean = False
        Dim NbRetries As Integer = 0

        bLoad = _MeteoArrays(MeteoIndex) Is Nothing
        bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex) Is Nothing
        bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex).GribDate < GetCurGribDate(Now)
        bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex).Dir = MeteoInfo.NO_VALUE
        'bLoad = bLoad OrElse Now.Ticks > NextTick

        If bLoad Then
            'RaiseEvent log("Synclock grib monitor th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
            While Not System.Threading.Monitor.TryEnter(_GribMonitor)

                If NoLock Then
                    Return Nothing
                End If

                NbRetries += 1
                If NbRetries = 3 Then
                    Return False
                End If
                System.Threading.Thread.Sleep(250)
            End While
            'SyncLock _GribMonitor
            Try
                bLoad = _MeteoArrays(MeteoIndex) Is Nothing
                bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex) Is Nothing
                bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex).GribDate < GetCurGribDate(Now)
                bLoad = bLoad OrElse _MeteoArrays(MeteoIndex).Data(LonIndex, latIndex).Dir = MeteoInfo.NO_VALUE
                'bLoad = bLoad OrElse Now.Ticks > NextTick
                If bLoad Then
                    'System.Threading.Monitor.Enter(_GribMonitor)
                    retval = LoadGribData(MeteoIndex, MeteoArray.GetArrayIndexLon(LonIndex, GetIndexGrain(MeteoIndex)), MeteoArray.GetArrayIndexLat(latIndex, GetIndexGrain(MeteoIndex)))
                Else
                    retval = True
                End If
            Catch ex As Exception
                RaiseEvent log("Exception during checkgribdata " & ex.Message)
                retval = False
            Finally
                System.Threading.Monitor.Exit(_GribMonitor)

            End Try
            'NextTick = Now.AddMinutes(30).Ticks
            Return retval
            'End SyncLock


        Else
            Return True
        End If
    End Function

    Public Shared Function GetCurGribDate(ByVal Dte As DateTime) As DateTime
        Dim CurZulu As DateTime = Dte.AddHours(GRIB_OFFSET + ZULU_OFFSET - 0.5)

        Return New DateTime(CurZulu.Year, CurZulu.Month, CurZulu.Day, CInt(Math.Floor(CurZulu.Hour / GRIB_PERIOD) * GRIB_PERIOD), 0, 0)

    End Function


    Private Function GetFileStream(ByVal MeteoIndex As Integer, ByVal Lon As Double, ByVal lat As Double) As String

        Dim RetString As String = ""
        Dim GribDataFileName As String = GetGribDataFileName(MeteoIndex)

        If Not System.IO.Directory.Exists(".\GribData") Then
            System.IO.Directory.CreateDirectory(".\GribData")
        End If

        If Not System.IO.File.Exists(System.IO.Path.Combine(".\GribData", GribDataFileName)) Then

            Dim FileOK As Boolean = False
            Dim WLon As Integer = CInt(-Lon - 5) 'revert grib lon for request!!!
            Dim ELon As Integer = WLon + 10
            Dim NLat As Integer = CInt(lat + 5)
            Dim SLat As Integer = NLat - 10
            Dim retries As Integer = 0
            Dim wr As WebResponse = Nothing
            Dim rs As System.IO.Stream
            Dim WaitStart As DateTime = Now


            While Not FileOK
                Dim GribURL As String = GetGribURL(MeteoIndex, WLon, ELon, NLat, SLat, False)
                'Dim Http As HttpWebRequest = CType(WebRequest.Create(New Uri(GribURL)), HttpWebRequest)
                Dim ftp As FtpWebRequest = CType(WebRequest.Create(New Uri(GribURL)), FtpWebRequest)
                Try

                    wr = ftp.GetResponse()
                    FileOK = True
                Catch ex2 As WebException
                    If retries > 2 Then
                        'Meteo is not available for that date (two late??)
                        Return ""
                    ElseIf ex2.Message.Contains("404") Then
                        retries += 1
                    End If
                Catch ex As Exception
                    Dim i As Integer = 0
                    Throw
                End Try
            End While
            rs = wr.GetResponseStream()
            Dim Gribdata(1024) As Byte
            Dim readlen As Integer
            Try
                Dim fName As String = "fgrib" & Now.Ticks  'System.IO.Path.GetTempFileName()
                Dim f As New System.IO.FileStream(fName, IO.FileMode.Create)
                Do
                    readlen = rs.Read(Gribdata, 0, 1024)
                    If readlen > 0 Then
                        f.Write(Gribdata, 0, readlen)
                    End If
                Loop Until readlen = 0

                f.Close()

                Dim App As New Process
                Dim Si As New System.Diagnostics.ProcessStartInfo

                With Si
                    .Arguments = """" & fName & """ -csv " & fName & ".csv"""
                    .FileName = "..\grib2\wgrib2.exe"
                    .UseShellExecute = False
                    .CreateNoWindow = True
                End With

                App.StartInfo = Si
                App.Start()

                'App.WaitForExit(0)
                WaitStart = Now
                While Not System.IO.File.Exists(fName & ".csv")
                    If Now.Subtract(WaitStart).TotalSeconds > 15 Then
                        Exit While
                    End If


                End While

                If Not System.IO.File.Exists(fName & ".csv") Then
                    Return ""
                End If

                Dim f2 As System.IO.StreamReader = Nothing

                WaitStart = Now
                While f2 Is Nothing
                    Try
                        f2 = New System.IO.StreamReader(fName & ".csv")
                        If Now.Subtract(WaitStart).TotalSeconds > 15 Then
                            Exit While
                        End If
                        System.Threading.Thread.Sleep(500)
                    Catch ex As Exception
                        System.Threading.Thread.Sleep(100)
                    End Try
                End While

                If f2 Is Nothing Then
                    Return ""
                End If

                Dim foutput As New System.IO.StreamWriter(System.IO.Path.Combine(".\GribData", GribDataFileName))
                foutput.Write(f2.ReadToEnd)
                foutput.Close()
                f2.Close()
                App.Dispose()
                System.IO.File.Delete(fName & ".csv")
                System.IO.File.Delete(fName)

            Catch ex As Exception
                RaiseEvent log("GetFileStream exception : " & ex.Message)
            End Try


        End If

        Return System.IO.Path.Combine(".\GribData", GribDataFileName)



    End Function


    Private Function GetGribDataFileName(ByVal MeteoIndex As Integer) As String

        Dim HourOffset As Integer
        Dim CurGrib As DateTime = GetCurGribDate(Now)

        If MeteoIndex < MAX_INDEX_05 Then
            HourOffset = MeteoIndex * GRIB_GRAIN_05
        Else
            HourOffset = MeteoIndex * GRIB_GRAIN_1
        End If


        Return "F" & CurGrib.ToString("yyyyMMddHH") & "." & HourOffset.ToString("00")

    End Function

    Private Function GetGribURL(ByVal MeteoIndex As Integer, ByVal WestLon As Integer, ByVal EastLon As Integer, ByVal NorthLat As Integer, ByVal SouthLat As Integer, ByVal UseHttp As Boolean) As String
        Dim CurGrib As DateTime = GetCurGribDate(Now)

        Dim HourOffset As Double
        Dim RequestDate As DateTime = CurGrib

        If UseHttp Then
            Dim ReturnUrl As String
            If MeteoIndex < MAX_INDEX_05 Then
                HourOffset = MeteoIndex * GRIB_GRAIN_05
                ReturnUrl = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs_hd.pl?file=gfs.t" & RequestDate.Hour.ToString("00") & _
                        "z.mastergrb2f" & (HourOffset).ToString("00") & "&lev_10_m_above_ground=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon=" & WestLon & "&rightlon=" & EastLon _
                        & "&toplat=" & NorthLat & "&bottomlat=" & SouthLat & "&dir=%2Fgfs." & RequestDate.ToString("yyyyMMddHH") & "%2Fmaster"
            Else
                HourOffset = 192 + GRIB_GRAIN_1 * (MeteoIndex - MAX_INDEX_05)
                ReturnUrl = "http://nomads.ncep.noaa.gov/cgi-bin/filter_gfs.pl?file=gfs.t" & RequestDate.Hour.ToString("00") & _
                        "z.pgrbf" & (HourOffset).ToString("00") & ".grib2&lev_10_m_above_ground=on&var_UGRD=on&var_VGRD=on&subregion=&leftlon=" & WestLon & "&rightlon=" & EastLon _
                        & "&toplat=" & NorthLat & "&bottomlat=" & SouthLat & "&dir=%2Fgfs." & RequestDate.ToString("yyyyMMddHH")
            End If

            Return ReturnUrl
        Else

            Dim ReturnUrl As String = "ftp://ftp.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs." & RequestDate.ToString("yyyyMMddHH") & "/gfs.t" & RequestDate.Hour.ToString("00") & _
                        "z.master.grbf" & (HourOffset).ToString("00") & ".10m.uv.grib2"
            Return ReturnUrl
        End If
    End Function

    Private Shared ReadOnly Property GetIndexGrain(ByVal MeteoIndex As Integer) As Double
        Get
            If MeteoIndex < MAX_INDEX_05 Then
                Return 0.5
            Else
                Return 2.5
            End If

        End Get
    End Property

    Private Function GetMeteoIndex(ByVal Dte As DateTime) As Integer
        Dim CurGrib As DateTime = GetCurGribDate(Now)

        If Dte < Now.AddDays(-1) Then
            Return 0
        End If

        Dim TotalHours As Double = Dte.AddHours(-_GMT_Offset).Subtract(CurGrib).TotalHours
        Dim HourIndex As Integer

        If TotalHours < MAX_INDEX_05 * GRIB_GRAIN_05 Then
            HourIndex = CInt(Math.Floor(TotalHours / GRIB_GRAIN_05))
        Else
            HourIndex = MAX_INDEX_05 + CInt(Math.Floor((TotalHours - 192) / GRIB_GRAIN_1))
        End If

        If HourIndex < 0 Then
            'Throw New ArgumentException("Dte before curgrib")
            Return 0
        End If

        If HourIndex >= MAX_INDEX Then
            Return MAX_INDEX
        Else
            Return HourIndex
        End If

    End Function

    Private Function GetMetoDateOffset(ByVal Dte As DateTime) As Double
        Dim CurGrib As DateTime = GetCurGribDate(Now)
        Dim TotalHours As Double = Dte.AddHours(-_GMT_Offset).Subtract(CurGrib).TotalHours

        If TotalHours <= MAX_GRIB_05 Then
            Return TotalHours Mod GRIB_GRAIN_05
        Else
            Return TotalHours Mod GRIB_GRAIN_1
        End If

    End Function

    Private Function SimpleAverageAngle(ByVal D0 As Double, ByVal D1 As Double, ByVal DX As Double) As Double
        If Math.Abs(D1 - D0) <= 180 Then
            Return CheckAngleInterp(D0 + DX * (D1 - D0))
        Else
            Return CheckAngleInterp(D0 + DX * CheckAngleInterp(D1 - D0))
        End If

    End Function
    Private Function SimpleAverage(ByVal V00 As Double, ByVal V01 As Double, ByVal dX As Double) As Double

        Return V00 + dX * (V01 - V00)

    End Function

    Private Function QuadraticAverageAngle(ByVal V00 As Double, ByVal V01 As Double, ByVal v10 As Double, ByVal V11 As Double, ByVal dX As Double, ByVal dY As Double, Optional ByRef Dir As DirInterpolation = 0) As Double
        Dim V0 As Double
        Dim V1 As Double

        V0 = V00 + dY * CheckAngleInterp(V01 - V00)


        V1 = v10 + dY * CheckAngleInterp(V11 - v10)

        Dir = If(CheckAngleInterp(V0) * CheckAngleInterp(V1) > 0, DirInterpolation.Pos, DirInterpolation.Neg)

        Return V0 + dX * CheckAngleInterp(V1 - V0)

    End Function

    Private Function QuadraticAverage(ByVal V00 As Double, ByVal V01 As Double, ByVal v10 As Double, ByVal V11 As Double, ByVal dX As Double, ByVal dY As Double) As Double
        Dim V0 As Double
        Dim V1 As Double

        V0 = V00 + dY * (V01 - V00)


        V1 = v10 + dY * (V11 - v10)


        Return V0 + dX * (V1 - V0)

    End Function
    Public Shared Function CheckAngleInterp(ByVal a As Double) As Double

        If a > 180 Then
            a -= 360
        ElseIf a < -180 Then
            a += 360
        End If

        Return a
    End Function

    Private Sub TransformBackUV(ByVal Dir As Double, ByVal Speed As Double, ByRef u As Double, ByRef v As Double)

        u = -Speed * Math.Sin(Dir / 180 * Math.PI) / 3.6 * 1.852
        v = -Speed * Math.Cos(Dir / 180 * Math.PI) / 3.6 * 1.852
        'Dim t_speed As Double = -Speed * Math.Sin(Dir / 180 * Math.PI) / 3.6 * 1.852
        'u = -Speed * Math.Cos(Dir / 180 * Math.PI) / 3.6 * 1.852
        'v = t_speed
        Return
    End Sub


    Private Function GetMeteoToIndex(ByVal MeteoIndex As Integer, ByRef Dir As DirInterpolation, ByVal Lon As Double, ByVal Lat As Double, ByVal NoLock As Boolean) As MeteoInfo

        Dim Lon0 As Integer = MeteoArray.GetLonArrayIndex(Lon, GetIndexGrain(MeteoIndex))
        Dim Lat0 As Integer = MeteoArray.GetLatArrayIndex(Lat, GetIndexGrain(MeteoIndex))
        Dim dX As Double = ((Lon - MeteoArray.GetArrayIndexLon(Lon0, GetIndexGrain(MeteoIndex))) Mod 360) / GetIndexGrain(MeteoIndex)
        Dim dY As Double = (Lat - MeteoArray.GetArrayIndexLat(Lat0, GetIndexGrain(MeteoIndex))) / GetIndexGrain(MeteoIndex)


        If Not CheckGribData(MeteoIndex, Lon0, Lat0, NoLock) Then
            Return Nothing
        End If
        'While Not CheckGribData(MeteoIndex, Lon0, Lat0) AndAlso MeteoIndex > 0
        '    MeteoIndex -= 1
        'End While

        If MeteoIndex < 0 Then
            Return Nothing
        End If




        Dim Lon1 As Integer = (Lon0 + MeteoArray.GetMaxLonindex(GetIndexGrain(MeteoIndex)) + 1) Mod (MeteoArray.GetMaxLonindex(GetIndexGrain(MeteoIndex)))
        Dim Lat1 As Integer = (Lat0 + MeteoArray.GetMaxLatindex(GetIndexGrain(MeteoIndex)) + 1) Mod (MeteoArray.GetMaxLatindex(GetIndexGrain(MeteoIndex)))

        If Not CheckGribData(MeteoIndex, Lon0, Lat1, NoLock) OrElse _
            Not CheckGribData(MeteoIndex, Lon1, Lat0, NoLock) OrElse _
            Not CheckGribData(MeteoIndex, Lon1, Lat1, NoLock) Then
            Return Nothing
        End If
        If _MeteoArrays(MeteoIndex).Data(Lon1, Lat0) Is Nothing Then
            Dim i As Long = 0
            i = Now.Ticks
        End If
        Dim D00 As Double = _MeteoArrays(MeteoIndex).Data(Lon0, Lat0).Dir
        Dim D01 As Double = _MeteoArrays(MeteoIndex).Data(Lon0, Lat1).Dir
        Dim D10 As Double = _MeteoArrays(MeteoIndex).Data(Lon1, Lat0).Dir
        Dim D11 As Double = _MeteoArrays(MeteoIndex).Data(Lon1, Lat1).Dir

        Dim S00 As Double = _MeteoArrays(MeteoIndex).Data(Lon0, Lat0).Strength
        Dim S01 As Double = _MeteoArrays(MeteoIndex).Data(Lon0, Lat1).Strength
        Dim S10 As Double = _MeteoArrays(MeteoIndex).Data(Lon1, Lat0).Strength
        Dim S11 As Double = _MeteoArrays(MeteoIndex).Data(Lon1, Lat1).Strength

        Dim Angle1 As Double = CheckAngleInterp(D01 - D00)
        Dim Angle2 As Double = CheckAngleInterp(D11 - D10)


        If Angle1 * Angle2 > 0 Then
            'Normal case, return twsa
            Dim retmeteo As New MeteoInfo() With {.Dir = QuadraticAverageAngle(D00, D01, D10, D11, dX, dY, Dir), _
                                         .Strength = QuadraticAverage(S00, S01, S10, S11, dX, dY)}

            If retmeteo.Dir < 0 Then
                retmeteo.Dir += 360
            ElseIf retmeteo.Dir > 360 Then
                retmeteo.Dir -= 360
            End If
            'Dir = If(Angle1 >= 0, DirInterpolation.Pos, DirInterpolation.Neg)
            Return retmeteo
        Else
            Dim D1 As Double = SimpleAverageAngle(D00, D01, dY)
            Dim D2 As Double = SimpleAverageAngle(D10, D11, dY)
            Dim S1 As Double = SimpleAverage(S00, S01, dY)
            Dim S2 As Double = SimpleAverage(S10, S11, dY)

            Dim u1 As Double
            Dim v1 As Double
            Dim u2 As Double
            Dim v2 As Double

            TransformBackUV(D1, S1, u1, v1)
            TransformBackUV(D2, S2, u2, v2)
            Dir = DirInterpolation.UV

            Return New MeteoInfo() With {.UGRD = SimpleAverage(u1, u2, dX), _
                                          .VGRD = SimpleAverage(v1, v2, dX)}

        End If
    End Function

    Public Function GetMeteoToDate(ByVal Dte As DateTime, ByVal Lon As Double, ByVal Lat As Double, ByVal NoLock As Boolean) As MeteoInfo

        Dim MeteoIndex = GetMeteoIndex(Dte)
        Dim NextMeteoIndex = MeteoIndex + 1
        Dim DteOffset As Double = GetMetoDateOffset(Dte) ' - GRIB_GRAIN * MeteoIndex
        Dim GribGrain As Double

        If NextMeteoIndex > MAX_INDEX Then
            NextMeteoIndex = MAX_INDEX
        End If

        If NextMeteoIndex < MAX_INDEX_05 Then
            GribGrain = GRIB_GRAIN_05
        Else
            GribGrain = GRIB_GRAIN_1
        End If

        Dim Dir0 As DirInterpolation
        Dim Dir1 As DirInterpolation

        Dim M0 As MeteoInfo = GetMeteoToIndex(MeteoIndex, Dir0, Lon, Lat, NoLock)
        Dim M1 As MeteoInfo = GetMeteoToIndex(NextMeteoIndex, Dir1, Lon, Lat, NoLock)

        If M0 Is Nothing OrElse M1 Is Nothing Then
            Return Nothing
        End If

        Dim RetInfo As New MeteoInfo

        If Dir0 = Dir1 OrElse Dir0 = DirInterpolation.UV OrElse Dir1 = DirInterpolation.UV Then
            RetInfo.Dir = M0.Dir + DteOffset / GribGrain * CheckAngleInterp(M1.Dir - M0.Dir)
            RetInfo.Strength = M0.Strength + DteOffset / GribGrain * (M1.Strength - M0.Strength)
        Else
            Dim u1 As Double
            Dim u2 As Double
            Dim v1 As Double
            Dim v2 As Double

            TransformBackUV(M0.Dir, M0.Strength, u1, v1)
            TransformBackUV(M1.Dir, M1.Strength, u2, v2)
            RetInfo.UGRD = SimpleAverage(u1, u2, DteOffset / GribGrain)
            RetInfo.VGRD = SimpleAverage(v1, v2, DteOffset / GribGrain)

        End If

        If RetInfo.Dir < 0 Then
            RetInfo.Dir += 360
        ElseIf RetInfo.Dir >= 360 Then
            RetInfo.Dir -= 360
        End If
        Return RetInfo
    End Function

    Private Function LoadGribDataFromFtp(ByVal MeteoIndex As Integer, ByVal lon As Double, ByVal lat As Double) As Boolean

        Dim GribDataFileName As String = GetFileStream(MeteoIndex, lon, lat)
        If GribDataFileName Is Nothing OrElse GribDataFileName = "" Then
            Return Nothing
        End If

        Dim fRead As New System.IO.StreamReader(GribDataFileName)
        Dim CurLon As Double = -1000
        Dim CurLat As Double = -1000
        Dim sb As New System.Text.StringBuilder
        Dim Line As String
        Dim FileOk As Boolean = True
        Dim FirstLine As Boolean

        While FileOk

            Line = fRead.ReadLine

            If Not Line = "" Then

                Dim Fields() As String = Line.Split(","c)

                If FirstLine Then
                    Dim CurGribDate As DateTime = New DateTime(CInt(Fields(0).Substring(1, 4)), _
                                                               CInt(Fields(0).Substring(6, 2)), _
                                                               CInt(Fields(0).Substring(9, 2)), _
                                                               CInt(Fields(0).Substring(12, 2)), 0, 0)

                    Dim DataGribDate As DateTime = New DateTime(CInt(Fields(1).Substring(1, 4)), _
                                                                           CInt(Fields(1).Substring(6, 2)), _
                                                                           CInt(Fields(1).Substring(9, 2)), _
                                                                           CInt(Fields(1).Substring(12, 2)), 0, 0)

                    If CurGribDate > _LastGribDate Then

                        For i = 0 To _MeteoArrays.Length - 1
                            _MeteoArrays(i) = Nothing
                        Next

                        _LastGribDate = CurGribDate
                        RaiseEvent log("Cleared meteo for new grib @   " & _LastGribDate.ToString)
                    End If
                    FirstLine = True
                End If

                'RaiseEvent log("Loaded @   " & DataGribDate.ToString & " lon " & lon & " , Lat " & lat)

                If _MeteoArrays(MeteoIndex) Is Nothing Then
                    _MeteoArrays(MeteoIndex) = New MeteoArray(GetIndexGrain(MeteoIndex))
                End If



                Dim Value As Double
                Dim Type As String = Fields(2)
                Dim GribDate As DateTime

                DateTime.TryParse(Fields(4), GribDate)

                Double.TryParse(Fields(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, CurLon)
                Double.TryParse(Fields(5), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, CurLat)
                Double.TryParse(Fields(6), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Value)

                'Grib lon are inverted
                CurLon = -CurLon

                If Math.Abs(lon - CurLon) < 5 AndAlso Math.Abs(lat - CurLat) < 5 Then


                    'If _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(CurLon), MeteoArray.GetLatArrayIndex(CurLat)) Is Nothing Then
                    '    _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(CurLon), MeteoArray.GetLatArrayIndex(CurLat)) = New MeteoInfo
                    'End If

                    Dim Data As MeteoInfo = _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(CurLon, GetIndexGrain(MeteoIndex)), MeteoArray.GetLatArrayIndex(CurLat, GetIndexGrain(MeteoIndex)))

                    Select Case Type
                        Case """UGRD"""
                            Data.UGRD = Value

                        Case """VGRD"""
                            Data.VGRD = Value

                    End Select
                End If

            End If

            FileOk = Not fRead.EndOfStream 'AndAlso CurLon <= lon + 5 AndAlso CurLat <= lat + 5

        End While

        Return True

    End Function
    Private Function LoadGribData(ByVal MeteoIndex As Integer, ByVal lon As Double, ByVal lat As Double) As Boolean

        Const SquareWidth As Integer = 20
        Dim WLon As Integer = CInt(lon - SquareWidth / 2) 'revert grib lon for request!!!
        Dim ELon As Integer = WLon + SquareWidth
        Dim NLat As Integer = CInt(lat + SquareWidth / 2)
        Dim SLat As Integer = NLat - SquareWidth
        Dim retries As Integer = 0
        Dim FileOK As Boolean = False
        Dim wr As WebResponse = Nothing
        Dim rs As System.IO.Stream
        Dim WaitStart As DateTime

        While Not FileOK
            Dim GribURL As String = GetGribURL(MeteoIndex, WLon, ELon, NLat, SLat, True)
            Dim Http As HttpWebRequest = CType(WebRequest.Create(New Uri(GribURL)), HttpWebRequest)
            Try
                Http.Timeout = 10000
                wr = Http.GetResponse()
                FileOK = True
            Catch ex2 As WebException
                If retries > 1 Then
                    'Meteo is not available for that date (two late??)
                    Return False
                ElseIf ex2.Message.Contains("404") Then
                    'retries += 1
                    Return False
                Else
                    Return False
                End If
            Catch ex As Exception
                Dim i As Integer = 0
                Throw
            Finally

            End Try
        End While
        rs = wr.GetResponseStream()


        Dim Gribdata(1024) As Byte
        Dim readlen As Integer
        Try
            If Not System.IO.Directory.Exists(RouteurModel.BaseFileDir) Then
                System.IO.Directory.CreateDirectory(RouteurModel.BaseFileDir)
            End If

            Dim fName As String = RouteurModel.BaseFileDir & "\fgrib" & Now.Ticks  'System.IO.Path.GetTempFileName()
            Dim f As New System.IO.FileStream(fName, IO.FileMode.Create)
            Do
                readlen = rs.Read(Gribdata, 0, 1024)
                If readlen > 0 Then
                    f.Write(Gribdata, 0, readlen)
                End If
            Loop Until readlen = 0

            f.Close()

            Dim Si As New System.Diagnostics.ProcessStartInfo

            With Si
                .Arguments = """" & fName & """ -csv """ & fName & ".csv"""
                .FileName = "..\grib2\wgrib2.exe"
                .UseShellExecute = False
                .CreateNoWindow = True
            End With

            _Process.StartInfo = Si
            _Process.EnableRaisingEvents = True
            _Process.Start()

            If Not _Evt.WaitOne(10000, Nothing) Then
                Return False
            End If

            'App.WaitForExit(0)
            WaitStart = Now
            While Not System.IO.File.Exists(fName & ".csv")
                If Now.Subtract(WaitStart).TotalSeconds > 15 Then
                    Exit While
                End If


            End While

            If Not System.IO.File.Exists(fName & ".csv") Then
                Return False
            End If

            Dim f2 As System.IO.StreamReader = Nothing

            WaitStart = Now
            While f2 Is Nothing
                Try
                    f2 = New System.IO.StreamReader(fName & ".csv")
                    If Now.Subtract(WaitStart).TotalSeconds > 15 Then
                        Exit While
                    End If
                    If f2 Is Nothing Then
                        System.Threading.Thread.Sleep(500)
                    End If
                Catch ex As Exception
                    System.Threading.Thread.Sleep(100)
                End Try
            End While

            If f2 Is Nothing Then
                Return False
            End If

            Dim S As String = f2.ReadToEnd
            f2.Close()
            '_Process.Dispose()

            Dim lines() As String = S.Split(CChar(vbLf))

            'Check gribdate
            Dim Fields() As String = lines(0).Split(","c)
            Dim CurGribDate As DateTime = New DateTime(CInt(Fields(0).Substring(1, 4)), _
                                                       CInt(Fields(0).Substring(6, 2)), _
                                                       CInt(Fields(0).Substring(9, 2)), _
                                                       CInt(Fields(0).Substring(12, 2)), 0, 0)

            Dim DataGribDate As DateTime = New DateTime(CInt(Fields(1).Substring(1, 4)), _
                                                                   CInt(Fields(1).Substring(6, 2)), _
                                                                   CInt(Fields(1).Substring(9, 2)), _
                                                                   CInt(Fields(1).Substring(12, 2)), 0, 0)

            If CurGribDate > _LastGribDate Then

                For i = 0 To _MeteoArrays.Length - 1
                    _MeteoArrays(i) = Nothing
                Next

                _LastGribDate = CurGribDate
                RaiseEvent log("Cleared meteo for new grib @   " & _LastGribDate.ToString)
            End If

            'RaiseEvent log("Loaded @   " & DataGribDate.ToString & " lon " & lon & " , Lat " & lat)

            If _MeteoArrays(MeteoIndex) Is Nothing Then
                _MeteoArrays(MeteoIndex) = New MeteoArray(GetIndexGrain(MeteoIndex))
            End If

            For Each Line In lines

                If Line.Trim <> "" Then
                    Fields = Line.Split(","c)

                    Dim lLon As Double
                    Dim lLat As Double
                    Dim Value As Double
                    Dim Type As String = Fields(2)
                    Dim GribDate As DateTime

                    DateTime.TryParse(Fields(4), GribDate)

                    Double.TryParse(Fields(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, lLon)
                    Double.TryParse(Fields(5), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, lLat)
                    Double.TryParse(Fields(6), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Value)

                    'Grib lon are inverted
                    'lLon = -lLon

                    'If _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(lLon), MeteoArray.GetLatArrayIndex(lLat)) Is Nothing Then
                    '    _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(lLon), MeteoArray.GetLatArrayIndex(lLat)) = New MeteoInfo
                    'End If

                    Dim Data As MeteoInfo = _MeteoArrays(MeteoIndex).Data(MeteoArray.GetLonArrayIndex(lLon, GetIndexGrain(MeteoIndex)), MeteoArray.GetLatArrayIndex(lLat, GetIndexGrain(MeteoIndex)))

                    Data.GribDate = CurGribDate
                    Select Case Type
                        Case """UGRD"""
                            Data.UGRD = Value

                        Case """VGRD"""
                            Data.VGRD = Value

                    End Select
                End If
            Next

            System.IO.File.Delete(fName & ".csv")
            System.IO.File.Delete(fName)
            Return True
        Catch ex As Exception
            RaiseEvent log("LoadGribDataExecption : " & ex.Message)
        End Try

        Return False

    End Function

    Public Sub New()


        Dim CurGrib As DateTime = GetCurGribDate(Now)
        _LastGribDate = New DateTime(0)

    End Sub

    Private Function get_ArrayAvg(ByVal Items As Double()) As Double
        Dim Sum As Double = 0
        Dim Count As Integer = 0

        If Items Is Nothing OrElse Items.GetUpperBound(0) < 0 Then
            Return 0
        Else
            For Each item In Items
                Sum += item
                Count += 1
            Next

            Return Sum / Count
        End If
    End Function

    Private Sub AddItemToArray(ByRef Array As Double(), ByVal value As Double, ByRef pointer As Integer)

        If Array Is Nothing OrElse Array.GetUpperBound(0) <= 0 Then
            ReDim Array(CORRECTION_LENGTH - 1)

            For i = 0 To CORRECTION_LENGTH - 1
                Array(i) = value
            Next
            pointer = 1
        Else
            Array(pointer) = value
            pointer = (pointer + 1) Mod CORRECTION_LENGTH
        End If

    End Sub


    Private Sub _Process_Exited(ByVal sender As Object, ByVal e As System.EventArgs) Handles _Process.Exited

        _Evt.Set()

    End Sub
End Class
