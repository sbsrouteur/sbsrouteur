Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml.Serialization
Imports System.Text

Public Class RecordedRoute

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event Log(Msg As String)

    Private _Key As Guid
    Private _RaceID As String
    Private _RaceName As String
    Private _RouteName As String
    Private _Route As ObservableCollection(Of RoutePointView)
    Private _Visible As Boolean = True
    Private _Color As Color = Color.FromRgb(CByte(Rnd() * 128 + 100), CByte(Rnd() * 128 + 100), CByte(Rnd() * 128 + 100))
    Private Shared _M As RouteurModel



    Public Sub New()
        _Key = New Guid
    End Sub

    Public Sub AddPoint(ByVal P As RoutePointView)

        Route.Add(P)
        AddHandler P.QueryRemoveFromRoute, AddressOf OnPointDeleteQuery

    End Sub

    Public Property Color() As Color
        Get
            Return _Color
        End Get
        Set(ByVal value As Color)
            _Color = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            OnShapeChange()
        End Set
    End Property


    Public ReadOnly Property ColorBrush() As SolidColorBrush
        Get
            _Color.A = 255
            Return New SolidColorBrush(Color)
        End Get
    End Property

    <XmlIgnore()> _
    Public Property ColorR() As Byte
        Get
            Return _Color.R
        End Get
        Set(ByVal value As Byte)
            _Color.R = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorR"))
            OnShapeChange()
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorG() As Byte
        Get
            Return _Color.G
        End Get
        Set(ByVal value As Byte)
            _Color.G = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorG"))
            OnShapeChange()
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorB() As Byte
        Get
            Return _Color.B
        End Get
        Set(ByVal value As Byte)
            _Color.B = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorB"))
            OnShapeChange()
        End Set
    End Property

    Public Sub ExportRoute_CSVFormat(ByVal OutputFile As String)
        Dim S1 As New System.IO.StreamWriter(OutputFile)

        For Each Pt In Route
            S1.WriteLine(Pt.ToString)
        Next

        S1.Close()

    End Sub

    Public Sub ExportRoute_XMLFormat(ByVal OutputFile As String)

        Dim E As New Xml.Serialization.XmlSerializer(GetType(RouteExport))

        Dim R As New RouteExport

        With R
            .Generator = New Generator
            Dim CurTicks As Long = CLng(Math.Floor(Now.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond)
            .Generator.ExportDate = ConvertToUTC(CurTicks)
            .Generator.Name = "SbsRouteur"
            .Generator.Version = My.Application.Info.Version.ToString

            .RouteList = New RouteExportRouteList

        End With

        R.RouteList.Route = New RouteElement
        With R.RouteList.Route
            .Name = RouteName
            .RaceID = RaceID
            .TrackColor = New RouteColor With {.A = Color.A, .R = Color.B, .G = Color.G, .B = Color.B}
            ReDim .Track(Route.Count - 1)

            For i As Integer = 0 To Route.Count - 1
                .Track(i) = New RoutePoint
                With .Track(i)
                    .ActionDate = ConvertToUTC(Route(i).ActionDate)
                    .ActionValue = New RoutePointActionValue
                    .ActionValue.Item = CType(Route(i).RouteValue, RoutePointDoubleValue).Value
                    .Coords = New RouteCoords() With {.Lat = Route(i).P.Lat_Deg, .Lon = Route(i).P.Lon_Deg}
                    .Type = RouteActionType.Bearing
                End With
            Next

        End With

        Try
            Using Sr As New IO.FileStream(OutputFile, IO.FileMode.Create)
                E.Serialize(Sr, R)
            End Using
        Catch ex As Exception
            MessageBox.Show("XML File export with reason : " & ex.Message)
        End Try
    End Sub

    Public Sub Initialize()

        For Each pt In Route
            AddHandler pt.QueryRemoveFromRoute, AddressOf OnPointDeleteQuery
            pt.IsPending = pt.ActionDate > Now
        Next

    End Sub

    <XmlIgnore()> _
    Public Property Model() As RouteurModel
        Get
            Return _M
        End Get
        Set(ByVal value As RouteurModel)
            _M = value
        End Set

    End Property

    Private Sub OnShapeChange()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Shape"))
    End Sub

    Public Property RaceID() As String
        Get
            Return _RaceID
        End Get
        Set(ByVal value As String)
            _RaceID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceID"))
        End Set
    End Property

    Public Property RaceName() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceName"))
        End Set
    End Property



    Public Sub RecomputeRoute(ByVal From As Coords, ByVal Meteo As clsMeteoOrganizer, ByVal BoatType As String, ByVal Sails As clsSailManager)

        Dim CurPos As Coords = New Coords(From)
        Dim CurDate As DateTime = Now
        Dim PrevPt As RoutePointView = Nothing

        For Each pt In _Route

            If pt.ActionDate >= CurDate Then
                If PrevPt IsNot Nothing Then

                    Select Case PrevPt.RouteMode

                        Case RoutePointView.EnumRouteMode.Bearing
                            pt.P = ComputeTrackBearing(PrevPt.P, CType(pt.RouteValue, RoutePointDoubleValue).Value, PrevPt.ActionDate, pt.ActionDate, Meteo, BoatType, Sails, True)
                        Case Else
                            MessageBox.Show("Routing mode not implemented yet, computation aborted")
                            Return
                    End Select

                End If

                PrevPt = pt

            End If

        Next
    End Sub

    Public Property Route() As ObservableCollection(Of RoutePointView)
        Get
            Return _Route
        End Get
        Set(ByVal value As ObservableCollection(Of RoutePointView))
            _Route = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Route"))
        End Set
    End Property

    Public Property RouteName() As String
        Get
            Return _RouteName
        End Get
        Set(ByVal value As String)
            _RouteName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteName"))
        End Set
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property Shape() As Shapes.Path
        Get
            If Not Visible Then
                Return Nothing
            End If
            Dim Ret As New Shapes.Path
            Ret.Stroke = ColorBrush
            Ret.StrokeThickness = 1
            Dim sb As New StringBuilder

            Try
                For Each Pt In Route
                    If sb.Length = 0 Then
                        sb.Append(" M ")
                    Else
                        sb.Append(" L ")
                    End If

                    sb.Append(GetCoordsString(Model.The2DViewer.LonToCanvas(Pt.P.Lon_Deg), Model.The2DViewer.LatToCanvas(Pt.P.Lat_Deg)))

                Next


                Dim PC As New PathFigureCollectionConverter

                Dim PG As New PathGeometry
                PG.Figures = CType(PC.ConvertFromString(sb.ToString), PathFigureCollection)
                Ret.Data = PG

            Catch ex As Exception
                'Swallow the exception. ry looging if anyone around
                RaiseEvent Log("Exception in Shape " & ex.Message & " : " & ex.StackTrace)
            End Try

            Return Ret
        End Get
    End Property

    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get

        Set(ByVal value As Boolean)
            If Visible <> value Then
                _Visible = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Visible"))
                OnShapeChange()
            End If
        End Set

    End Property

    Private Sub OnPointDeleteQuery(ByVal P As RoutePointView)

        If Route.Contains(P) Then
            RemoveHandler P.QueryRemoveFromRoute, AddressOf OnPointDeleteQuery
            Route.Remove(P)
            OnShapeChange()
        End If

    End Sub

    Private Function ConvertToUTC(ByVal DteTicks As Long) As Date
        Return ConvertToUTC(New DateTime(DteTicks))
    End Function


    Private Function ConvertToUTC(ByVal Dte As Date) As Date
        Dim OffSet As TimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(Dte)
        Return New DateTime(Dte.Add(-OffSet).Ticks, DateTimeKind.Utc)
    End Function

    Sub ExportRoute_JSONFormat(outputfile As String)
        Dim S1 As New System.IO.StreamWriter(outputfile)

        Dim Data As New List(Of Object)
        Dim Cvt As New EpochToUTCDateConverter

        For Each Pt In Route
            Dim JSonPoint(2) As String
            JSonPoint(0) = CStr(Cvt.ConvertBack(Pt.ActionDate.ToUniversalTime, GetType(Long), Nothing, System.Globalization.CultureInfo.CurrentCulture))
            JSonPoint(1) = (Pt.P.Lon_Deg * 1000).ToString(System.Globalization.CultureInfo.InvariantCulture)
            JSonPoint(2) = (Pt.P.Lat_Deg * 1000).ToString(System.Globalization.CultureInfo.InvariantCulture)
            Data.Add(JSonPoint)
        Next

        S1.Write(JSonHelper.GetStringFromJsonObject(Data))

        S1.Close()
    End Sub

End Class
