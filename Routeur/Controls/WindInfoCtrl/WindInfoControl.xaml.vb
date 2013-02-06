'This file is part of Routeur.
'Copyright (C) 2011  sbsRouteur(at)free.fr

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

Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.ComponentModel
Imports System.Windows.Threading

Partial Public Class WindInfoControl

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private WithEvents _Refresh As DispatcherTimer
    Private WithEvents _MeteoInfo As MeteoInfo

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
        _Refresh = New DispatcherTimer(New TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Normal, AddressOf _Refresh_Elapsed, Dispatcher)
    End Sub



    Public Property IsPrev As Boolean
        Get
            Return CBool(GetValue(IsPrevProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsPrevProperty, value)
        End Set
    End Property

    Public Shared ReadOnly IsPrevProperty As DependencyProperty = _
                           DependencyProperty.Register("IsPrev", _
                           GetType(Boolean), GetType(WindInfoControl), _
                           New FrameworkPropertyMetadata(Nothing))



    Public Property WindPos As Coords
        Get
            Return CType(GetValue(WindPosProperty), Coords)
        End Get

        Set(ByVal value As Coords)
            SetValue(WindPosProperty, value)
        End Set
    End Property

    Public Shared ReadOnly WindPosProperty As DependencyProperty = _
                           DependencyProperty.Register("WindPos", _
                           GetType(Coords), GetType(WindInfoControl), _
                           New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AddressOf OnWindPosChanged))

    Public Shared Sub OnWindPosChanged(o As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim ref As WindInfoControl = CType(o, WindInfoControl)
        ref.WindPos = CType(e.NewValue, Coords)
        ref._Refresh.Start()
    End Sub


    Public Property MeteoInfo As MeteoInfo
        Get
            Return _MeteoInfo
        End Get
        Set(value As MeteoInfo)
            _MeteoInfo = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoInfo"))
        End Set
    End Property

    Public Property MeteoProvider As GribManager
        Get
            Return CType(GetValue(MeteoProviderProperty), GribManager)
        End Get

        Set(ByVal value As GribManager)
            SetValue(MeteoProviderProperty, value)
        End Set
    End Property

    Public Shared ReadOnly MeteoProviderProperty As DependencyProperty = _
                           DependencyProperty.Register("MeteoProvider", _
                           GetType(GribManager), GetType(WindInfoControl), _
                           New FrameworkPropertyMetadata(Nothing))


    Public Property WindDate As DateTime
        Get
            Return CDate(GetValue(WindDateProperty))
        End Get

        Set(ByVal value As DateTime)
            SetValue(WindDateProperty, value)
        End Set
    End Property

    Public Shared ReadOnly WindDateProperty As DependencyProperty = _
                           DependencyProperty.Register("WindDate", _
                           GetType(DateTime), GetType(WindInfoControl), _
                           New FrameworkPropertyMetadata(AddressOf OnWindDateChanged))

    Private Shared Sub OnWindDateChanged(o As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim Ref As WindInfoControl = CType(o, WindInfoControl)
        Ref.WindDate = CDate(e.NewValue)
        Ref._Refresh.Start()
    End Sub

    Private Sub _Refresh_Elapsed(sender As Object, e As EventArgs)
        _Refresh.Stop()
        If MeteoProvider IsNot Nothing AndAlso WindDate.Ticks <> 0 AndAlso WindPos IsNot Nothing Then
            Dim Mi As MeteoInfo = MeteoProvider.GetMeteoToDate(WindPos, WindDate, True, True)
            If Mi IsNot Nothing Then
                MeteoInfo.Strength = Mi.Strength
                MeteoInfo.Dir = Mi.Dir
            End If
        End If
    End Sub

End Class
