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
    Private WithEvents _Route As ObservableCollection(Of RoutePointView)
    Private _Path As New LinkedList(Of Coords)
    Private _Visible As Boolean = True
    Private _Color As Color = Color.FromRgb(CByte(Rnd() * 128), CByte(Rnd() * 128), CByte(Rnd() * 128))
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

     Public Sub RecomputeRoute(ByVal From As Coords, ByVal Meteo As GribManager, ByVal BoatType As String, ByVal Sails As clsSailManager)

        Dim CurPos As Coords = New Coords(From)
        Dim CurDate As DateTime = Now
        Dim PrevPt As RoutePointView = Nothing

        'Reset Pos of all points in the future, except the first one

        For Each pt In (From Point In Route Where Point IsNot Route(0) And Point.ActionDate >= CurDate)
            pt.P = Nothing
        Next

        For Each pt In Route

            If pt.ActionDate >= CurDate Then
                If PrevPt IsNot Nothing AndAlso PrevPt.P IsNot Nothing Then

                    Select Case PrevPt.RouteMode

                        Case RoutePointView.EnumRouteMode.Bearing
                            pt.P = ComputeTrackBearing(PrevPt.P, CType(PrevPt.RouteValue, RoutePointDoubleValue).Value, PrevPt.ActionDate, pt.ActionDate, Meteo, BoatType, Sails, True)
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

    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get

        Set(ByVal value As Boolean)
            If Visible <> value Then
                _Visible = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Visible"))

            End If
        End Set

    End Property

    Private Sub OnPointDeleteQuery(ByVal P As RoutePointView)

        If Route.Contains(P) Then
            RemoveHandler P.QueryRemoveFromRoute, AddressOf OnPointDeleteQuery
            Route.Remove(P)

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

    <XmlIgnore> _
    ReadOnly Property Path As LinkedList(Of Coords)
        Get
            Return _Path
        End Get
    End Property

    <XmlIgnore> _
    ReadOnly Property PenColor() As Integer
        Get
            Return CInt(Color.A) << 24 Or CInt(Color.R) << 16 Or CInt(Color.G) << 8 Or Color.B
        End Get
    End Property

    Private Sub _Route_CollectionChanged(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs) Handles _Route.CollectionChanged
        RebuildPath()

    End Sub

    Private Sub RebuildPath()
        _Path.Clear()
        For Each item In Route
            _Path.AddLast(item.P)
        Next
    End Sub

    Sub ClearPastPoints()

        Dim DL = (From P In Route Where P.ActionDate < Now).ToList

        For Each P In DL
            Route.Remove(P)
        Next

    End Sub

End Class
