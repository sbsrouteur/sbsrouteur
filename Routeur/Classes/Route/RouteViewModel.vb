Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Routeur.RoutePointView

Public Class RouteViewModel

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Const NB_MAX_POINTS_PILOTOTO As Integer = 5

    Private _Name As String
    Private _IdUser As Integer
    Private WithEvents _Points As ObservableCollection(Of RoutePointView)
    Private _NbMaxPoints As Integer = -1

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property

    Public Property NbMaxPoints() As Integer
        Get
            Return _NbMaxPoints
        End Get
        Set(ByVal value As Integer)
            _NbMaxPoints = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NbMaxPoints"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CanAddPoints"))
        End Set
    End Property

    Public Property Points() As ObservableCollection(Of RoutePointView)
        Get
            Return _Points
        End Get
        Set(ByVal value As ObservableCollection(Of RoutePointView))
            _Points = value
        End Set
    End Property

    Private Sub _Points_CollectionChanged(ByVal sender As Object, ByVal e As System.Collections.Specialized.NotifyCollectionChangedEventArgs) Handles _Points.CollectionChanged

        Select Case e.Action
            Case Specialized.NotifyCollectionChangedAction.Add

                For Each item As RoutePointView In e.NewItems
                    AddHandler item.PropertyChanged, AddressOf ItemPropertyChanged
                Next

            Case Specialized.NotifyCollectionChangedAction.Remove
                For Each item As RoutePointView In e.OldItems
                    RemoveHandler item.PropertyChanged, AddressOf ItemPropertyChanged
                Next

        End Select

    End Sub

    Private Sub ItemPropertyChanged(ByVal item As Object, ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(item, e)

    End Sub


    Public ReadOnly Property CanAddPoints() As Boolean
        Get

            Return NbMaxPoints = -1 OrElse Points Is Nothing OrElse Points.Count < NbMaxPoints

        End Get
    End Property

    Public Sub AddPoint()

        Dim P As New RoutePointView(_IdUser, 0, RoutePointView.EnumRouteMode.Angle, Now, New RoutePointDoubleValue(0))
        Points.Add(P)

    End Sub

    Public Sub New(ByVal IdUser As Integer, ByVal Route() As String, ByVal MaxPointCount As Integer)

        _Points = New ObservableCollection(Of RoutePointView)
        NbMaxPoints = MaxPointCount
        _IdUser = IdUser

        For Each Point As String In Route

            Dim Fields() As String

            If Not Point Is Nothing Then
                Fields = Point.Split(","c)

                If Fields.Count >= MIN_PILOTOTO_FIED_COUNT Then
                    Dim OrderType As EnumRouteMode
                    Dim OrderDate As DateTime

                    If GetOrderType(Fields(FLD_ORDERTYPE), OrderType) Then

                        Dim PointView As RoutePointView = Nothing
                        Dim OrderID As Integer

                        GetOrderID(Fields(FLD_ID), OrderID)
                        GetOrderDate(Fields(FLD_DATEVALUE), OrderDate)

                        Select Case OrderType
                            Case EnumRouteMode.Bearing, EnumRouteMode.Angle
                                Dim Value As Double
                                GetBearingAngleValue(Fields(FLD_ORDERVALUE), Value)
                                PointView = New RoutePointView(IdUser, OrderID, OrderType, OrderDate, New RoutePointDoubleValue(Value))

                            Case EnumRouteMode.BVMG, EnumRouteMode.Ortho, EnumRouteMode.VMG
                                Dim Lon As Double
                                Dim Lat As Double
                                Dim WithBearing As Boolean
                                Dim Bearing As Double

                                GetWPInfo(Fields(FLD_LATVALUE), Fields(FLD_LONVALUE), Lon, Lat, WithBearing, Bearing)

                                Dim PointValue As New RoutePointWPValue(Lat, Lon, WithBearing, Bearing)
                                PointView = New RoutePointView(IdUser, OrderID, OrderType, OrderDate, PointValue)

                        End Select

                        If Not PointView Is Nothing Then
                            PointView.IsPending = OrderIsPending(Point)
                            Points.Add(PointView)
                        End If


                    End If
                End If
            End If

        Next

    End Sub

End Class
