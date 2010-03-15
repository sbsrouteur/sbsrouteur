Imports System.Net
Imports System.Xml.Serialization

Public Class CurrentMgr

    Private Class CurrentArray

        Private _StartCoords As Coords
        Private _CoordStep As Double
        Private _Data() As CurrentInfo
        Private _dx As Double
        Private _dy As Double
        Private _rows As Integer
        Private _cols As Integer

        Public Function GetCurrentData(ByVal C As Coords) As CurrentInfo

            If Not RouteurModel.HasCurrents Then
                Return Nothing
            End If

            If C.Lat_Deg > _StartCoords.Lat_Deg + _dy / 2 OrElse _
                C.Lat_Deg < _StartCoords.Lat_Deg - _cols * _dy - _dy / 2 OrElse _
                C.Lon_Deg > _StartCoords.Lon_Deg + _dx / 2 OrElse _
                C.Lon_Deg < _StartCoords.Lon_Deg + _cols * _dx + _dx / 2 Then
                Return Nothing
            Else

                Dim C1 As New Coords(C)
                C1.Lon_Deg = CInt(Math.Round(C1.Lon_Deg / _dx) * _dx)
                C1.Lat_Deg = CInt(Math.Round(C1.Lat_Deg / _dy) * _dy)

                Dim x As Integer = CInt(-(_StartCoords.Lon_Deg - C1.Lon_Deg) / _dx)
                Dim y As Integer = CInt((_StartCoords.Lat_Deg - C1.Lat_Deg) / _dy)

                Return _Data(_cols * y + x)

            End If


        End Function

        Public Sub LoadCurrent(ByVal StartCoords As Coords, ByVal dx As Double, ByVal dy As Double, ByVal rows As Integer, ByVal cols As Integer, ByVal Data As String)

            ReDim _Data(rows * cols)
            Dim fields() As String = Data.Split(" "c)
            Dim i As Integer

            _StartCoords = StartCoords
            _dx = dx
            _dy = dy
            _rows = rows
            _cols = cols

            For i = 0 To CInt(fields.Length / 2) - 1

                _Data(i) = New CurrentInfo With {.cap = CInt(CInt(fields(2 * i + 1)) + 180) Mod 360, .force = CDbl(fields(CInt(2 * i)))}

            Next


        End Sub

    End Class

    Private _CurrentPrevs(4) As CurrentArray
    Private _CurStart As DateTime = New Date(0)
    Private _WebClient As New webclient

    Private Function RefDate(ByVal dte As DateTime) As DateTime

        Return New Date(dte.Year, dte.Month, dte.Day, dte.Hour, 0, 0)


    End Function

    Public Function GetCurrentToDate(ByVal C As Coords, ByVal Dte As DateTime) As CurrentInfo

        Dim IndexDate As DateTime = RefDate(Dte)
        Dim PrevIndex As Integer = CInt(IndexDate.Subtract(RefDate(Now)).TotalHours)

        If Not RouteurModel.HasCurrents Then
            Return Nothing
        End If

        If _CurStart = New Date(0) OrElse _CurStart <> RefDate(Now) Then
            'reload 4 files
            _CurStart = RefDate(Now)
            For i = 0 To 3
                Try
                    Dim ResponseString = _WebClient.DownloadString(STR_GetCurrent(_CurStart.AddHours(i)))

                    Dim memstream As New System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(ResponseString))
                    Dim CurrentSerializer As New XmlSerializer(GetType(currents.PREVISIONS))
                    Dim errorcount As Integer
                    Dim Currents As currents.PREVISIONS

                    'While errorcount < 2
                    Try
                        Currents = CType(CurrentSerializer.Deserialize(memstream), currents.PREVISIONS)
                        _CurrentPrevs(i) = New CurrentArray
                        _CurrentPrevs(i).LoadCurrent(New Coords(Currents.LATITUDESTART, -Currents.LONGITUDESTART), -Currents.DX, Currents.DY, Currents.ROWS, Currents.COLS, Currents.PREVISION.Value)
                    Catch ex As Exception
                        _CurrentPrevs(i) = Nothing
                        errorcount += 1
                    End Try
                    'End While
                Catch ex As Exception
                End Try


            Next
        End If

        If PrevIndex < 0 OrElse PrevIndex > 3 Then


            Return Nothing
        Else
            Return _CurrentPrevs(PrevIndex).GetCurrentData(C)
        End If


    End Function

    Private Function STR_GetCurrent(ByVal Dte As DateTime) As String

        Return RouteurModel.BASE_GAME_URL & "/resources/currents/" & Dte.ToString("yyyy_MM_dd_HH") & ".xml?ver=17_16_" & CInt(Rnd() * 99999)

    End Function

End Class

Public Class CurrentInfo

    Public cap As Integer
    Public force As Double

End Class
