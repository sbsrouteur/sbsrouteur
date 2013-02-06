﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class Stats
    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Shared _Stats As New SortedList(Of StatID, StatInfo)
    Private WithEvents _Refresh As New Timers.Timer With {.Interval = 1000, .Enabled = True}
    Private Shared _Changed As Boolean = False


    Public Enum StatID As Integer
        BSPCellCount
        BSPCellAvgDepth
        BSP_GetSegAvgMS
        BSP_GetSegHitRatio
        HitTestBspAvgMS
        HitTestBSPAvgLoops
        HitTestNoBspAvgMS
        HitTestNoBSPAvgLoops
        GridAvgProcesLengthMS
        GridAvgBuildListMS
        GridAvg_CheckSegmentMS
        GridAvgGetETAMS
        GridAvgGetETALoops
        HitDistanceAvgMS
        HitDistanceAvgLoops
        HitDistanceAvgPolyCount
        Grib_GetMeteoToDateAvgMS
        Polar_CacheRatio
        DRAW_FPS
        RIndex_AvgQueryTimeMS
        RIndex_AvgHitCount
        RIndex_AvgHitCountNonZero
        RIndex_ExceptionRatio
        ISO_ReachPointMS
        ISO_ReachPointMeteoWaitMS
        ISO_ReachPointAvgLoopCount
    End Enum

    Public Shared ReadOnly Property Stats() As ObservableCollection(Of StatInfo)
        Get

            Return New ObservableCollection(Of StatInfo)(_Stats.Values)
        End Get
    End Property



    Private Sub _Refresh_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _Refresh.Elapsed
        If _Changed Then
            _Changed = False
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Stats"))
        End If
    End Sub

    Public Shared WriteOnly Property SetStatValue(ByVal Prop As StatID) As Double
        Set(ByVal value As Double)
            Dim Item As StatInfo
            SyncLock _Stats
                If _Stats.ContainsKey(Prop) Then
                    Item = _Stats(Prop)
                    If Item.Value <> value Then
                        Item.Value = value
                        _Changed = True
                    End If
                Else
                    Item = New StatInfo With {.Name = Prop.ToString, .Value = value}
                    _Changed = True
                    _Stats.Add(Prop, Item)
                End If
            End SyncLock
        End Set
    End Property

End Class

Public Class StatInfo


    Private _Name As String
    Private _Value As Double

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)

            If value <> Name Then
                _Name = value
            End If
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return Name & " :  " & Value
    End Function

    Public Property Value() As Double
        Get
            Return _Value
        End Get
        Set(ByVal value As Double)

            If value <> _Value Then
                _Value = value
            End If
        End Set
    End Property



End Class
