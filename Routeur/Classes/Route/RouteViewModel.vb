Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Routeur.RoutePointViewBase

Public Class RouteViewModel

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Name As String
    Private WithEvents _Points As ObservableCollection(Of RoutePointViewBase)

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property

    Public Property Points() As ObservableCollection(Of RoutePointViewBase)
        Get
            Return _Points
        End Get
        Set(ByVal value As ObservableCollection(Of RoutePointViewBase))
            _Points = value
        End Set
    End Property

    Private Sub _Points_CollectionChanged(ByVal sender As Object, ByVal e As System.Collections.Specialized.NotifyCollectionChangedEventArgs) Handles _Points.CollectionChanged

        Select Case e.Action
            Case Specialized.NotifyCollectionChangedAction.Add

                For Each item As RoutePointViewBase In e.NewItems
                    AddHandler item.PropertyChanged, AddressOf ItemPropertyChanged
                Next

            Case Specialized.NotifyCollectionChangedAction.Remove
                For Each item As RoutePointViewBase In e.OldItems
                    RemoveHandler item.PropertyChanged, AddressOf ItemPropertyChanged
                Next

        End Select

    End Sub

    Private Sub ItemPropertyChanged(ByVal item As Object, ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(item, e)

    End Sub

    Public Sub New()

    End Sub

    Public Sub New(ByVal Route() As String)

        _Points = New ObservableCollection(Of RoutePointViewBase)

        For Each Point As String In Route

            Dim Fields() As String

            If Not Point Is Nothing Then
                Fields = Point.Split(","c)

                If Fields.Count >= MIN_PILOTOTO_FIED_COUNT Then
                    Dim OrderType As EnumRouteMode
                    Dim OrderDate As DateTime

                    If GetOrderType(Fields(FLD_ORDERTYPE), OrderType) Then

                        Dim PointView As RoutePointViewBase = Nothing
                        GetOrderDate(Fields(FLD_DATEVALUE), OrderDate)

                        Select Case OrderType
                            Case EnumRouteMode.Bearing
                                Dim Value As Double
                                GetBearingAngleValue(Fields(FLD_ORDERVALUE), Value)
                                PointView = New RouteBearingPointView(OrderDate, New RoutePointDoubleValue(Value))

                            Case EnumRouteMode.Angle
                                Dim Value As Double
                                GetBearingAngleValue(Fields(FLD_ORDERVALUE), Value)
                                PointView = New RouteAnglePointView(OrderDate, New RoutePointDoubleValue(Value))


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
