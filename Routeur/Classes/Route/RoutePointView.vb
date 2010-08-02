Imports System.ComponentModel

Public Class RoutePointView

    
    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Enum EnumRouteMode As Integer

        Bearing = 1
        Angle = 2
        Ortho = 3
        VMG = 4
        VBVMG = 5

    End Enum

    Private _RouteMode As EnumRouteMode
    Private _ActionDate As DateTime
    Private _Pending As Boolean
    Private _ID As Integer
    Private _UserID As Integer
    Private _NeedUpdate As Boolean
    Private WithEvents _routevalue As RoutePointValueBase


    Public Property ActionDate() As DateTime
        Get
            Return _ActionDate
        End Get
        Set(ByVal value As DateTime)
            _ActionDate = value
            NeedUpdate = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ActionDate"))
        End Set
    End Property

    Public ReadOnly Property CanDelete() As Boolean
        Get
            Return _ID <> 0
        End Get
    End Property
    Public Sub Delete()

        If ID = 0 Then
            Return
        End If

        'Create the delete query
        Dim Data As New Dictionary(Of String, Object)
        Data.Add("taskid", ID)
        Data.Add("idu", UserID)
        Data.Add("debug", False)


        WS_Wrapper.PostBoatSetup("pilototo_delete", GetStringFromJsonObject(Data))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UPLOAD"))


    End Sub

    Public ReadOnly Property EnumRouteModes() As String()
        Get

            Return [Enum].GetNames(GetType(EnumRouteMode))

        End Get
    End Property

    Public ReadOnly Property HasDoubleValue() As Visibility
        Get

            If RouteMode = EnumRouteMode.Angle OrElse RouteMode = EnumRouteMode.Bearing Then
                Return Visibility.Visible
            Else
                Return Visibility.Collapsed
            End If
        End Get
    End Property

    Public ReadOnly Property HasWPValue() As Visibility
        Get

            If RouteMode = EnumRouteMode.Angle OrElse RouteMode = EnumRouteMode.Bearing Then
                Return Visibility.Collapsed
            Else
                Return Visibility.Visible
            End If
        End Get
    End Property

    Public Property ID() As Integer
        Get
            Return _ID
        End Get
        Set(ByVal value As Integer)
            _ID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ID"))
        End Set
    End Property
    Public Property IsPending() As Boolean
        Get
            Return _Pending
        End Get
        Set(ByVal value As Boolean)
            _Pending = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsPending"))
        End Set
    End Property

    Public Property NeedUpdate() As Boolean
        Get
            Return _NeedUpdate
        End Get
        Set(ByVal value As Boolean)
            _NeedUpdate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NeedUpdate"))
        End Set
    End Property

    Public Property RouteMode() As EnumRouteMode
        Get
            Return _RouteMode
        End Get
        Set(ByVal value As EnumRouteMode)

            Dim valueiswp As Boolean = Not (value = EnumRouteMode.Angle Or value = EnumRouteMode.Bearing)
            Dim previswp As Boolean = Not (_RouteMode = EnumRouteMode.Angle Or _RouteMode = EnumRouteMode.Bearing)
            If valueiswp <> previswp Then

                If value = EnumRouteMode.Angle OrElse value = EnumRouteMode.Bearing Then
                    RouteValue = New RoutePointDoubleValue(0)
                Else
                    RouteValue = New RoutePointWPValue
                End If
            End If
            _RouteMode = value
            NeedUpdate = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteMode"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HasDoubleValue"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HasWPValue"))

        End Set
    End Property

    Public Property RouteValue() As RoutePointValueBase
        Get
            Return _routevalue
        End Get

        Set(ByVal value As RoutePointValueBase)

            _routevalue = value
            NeedUpdate = True
            OnPropertyChanged(New PropertyChangedEventArgs("RouteValue"))
        End Set

    End Property

    Protected Sub OnPropertyChanged(ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, e)

    End Sub

    'Public Overrides Function SelectTemplate(ByVal item As Object, ByVal container As System.Windows.DependencyObject) As System.Windows.DataTemplate

    '    Dim element As FrameworkElement
    '    element = TryCast(container, FrameworkElement)

    '    If element IsNot Nothing AndAlso item IsNot Nothing Then
    '        Select Case CType(item, RoutePointView).RouteMode
    '            Case EnumRouteMode.Angle, EnumRouteMode.Bearing
    '                Return DirectCast(element.FindResource("DoubleValueTemplate"), DataTemplate)

    '            Case EnumRouteMode.BVMG, EnumRouteMode.VMG, EnumRouteMode.Ortho
    '                Return DirectCast(element.FindResource("WPValueTemplate"), DataTemplate)

    '        End Select

    '    End If

    '    Return MyBase.SelectTemplate(item, container)

    'End Function

    Public Sub SetDraggedValue(ByVal P As Coords)

        If RouteMode = EnumRouteMode.Angle OrElse RouteMode = EnumRouteMode.Bearing Then
        Else
            Dim WPValue As RoutePointWPValue = CType(RouteValue, RoutePointWPValue)
            WPValue.WPLat = P.Lat_Deg
            WPValue.WPLon = P.Lon_Deg

        End If
    End Sub

    Public ReadOnly Property TaskTime() As Long
        Get
            Return CLng(ActionDate.AddHours(GribManager.ZULU_OFFSET).Subtract(New DateTime(1970, 1, 1)).TotalSeconds)
        End Get
    End Property

    Public Sub Update()

        Dim Verb As String
        'Create the delete query
        Dim Data As New Dictionary(Of String, Object)
        Data.Add("idu", UserID)

        If ID <> 0 Then
            Data.Add("taskid", ID)
            Verb = "pilototo_update"
        Else
            Verb = "pilototo_add"
        End If
        Data.Add("pim", CInt(RouteMode))
        Select Case RouteMode
            Case EnumRouteMode.Angle, EnumRouteMode.Bearing
                Data.Add("pip", CType(RouteValue, RoutePointDoubleValue).Value.ToString("f1", System.Globalization.CultureInfo.InvariantCulture))

            Case EnumRouteMode.VBVMG, EnumRouteMode.Ortho, EnumRouteMode.VMG
                Dim WPData As New Dictionary(Of String, Object)
                Dim WPValue As RoutePointWPValue = CType(RouteValue, RoutePointWPValue)
                WPData.Add("targetlat", WPValue.WPLat)
                WPData.Add("targetlong", WPValue.WPLon)

                If WPValue.SetBearingAtWP Then
                    WPData.Add("targetandhdg", WPValue.BearingAtWP)
                Else
                    WPData.Add("targetandhdg", Nothing)
                End If

                Data.Add("pip", WPData)
        End Select

        Data.Add("tasktime", TaskTime)

        WS_Wrapper.PostBoatSetup(Verb, GetStringFromJsonObject(Data))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UPLOAD"))

    End Sub

    Public Property UserID() As Integer
        Get
            Return _UserID
        End Get
        Set(ByVal value As Integer)
            _UserID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UserID"))
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal UserID As Integer, ByVal PointID As Integer, ByVal PointType As EnumRouteMode, ByVal ActionDate As DateTime, ByVal value As RoutePointDoubleValue)

        Me.ActionDate = ActionDate
        RouteMode = PointType
        RouteValue = value
        Me.ID = PointID
        Me.UserID = UserID
        NeedUpdate = False

    End Sub
    Public Sub New(ByVal UserID As Integer, ByVal PointID As Integer, ByVal PointType As EnumRouteMode, ByVal ActionDate As DateTime, ByVal value As RoutePointWPValue)

        Me.ActionDate = ActionDate
        RouteMode = PointType
        RouteValue = value
        Me.ID = PointID
        Me.UserID = UserID
        NeedUpdate = False

    End Sub

    Private Sub _routevalue_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _routevalue.PropertyChanged

        NeedUpdate = True
        RaiseEvent PropertyChanged(sender, e)

    End Sub
End Class