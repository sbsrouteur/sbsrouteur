﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Ce code a été généré par un outil.
'     Version du runtime :2.0.50727.4016
'
'     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
'     le code est régénéré.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports System.Xml.Serialization

'
'This source code was auto-generated by xsd, Version=2.0.50727.3038.
'

'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038"),  _
 System.SerializableAttribute(),  _
 System.Diagnostics.DebuggerStepThroughAttribute(),  _
 System.ComponentModel.DesignerCategoryAttribute("code"),  _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=true),  _
 System.Xml.Serialization.XmlRootAttribute([Namespace]:="", IsNullable:=false)>  _
Partial Public Class PREVISIONS
    Inherits Object
    Implements System.ComponentModel.INotifyPropertyChanged
    
    Private pREVISIONField() As PREVISIONSPREVISION
    
    Private cOLSField As UShort
    
    Private rOWSField As UShort
    
    Private lONGITUDESTARTField As Decimal
    
    Private lATITUDESTARTField As Decimal
    
    Private dxField As Decimal
    
    Private dyField As Decimal
    
    '''<remarks/>
    <System.Xml.Serialization.XmlElementAttribute("PREVISION")>  _
    Public Property PREVISION() As PREVISIONSPREVISION()
        Get
            Return Me.pREVISIONField
        End Get
        Set
            Me.pREVISIONField = value
            Me.RaisePropertyChanged("PREVISION")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property COLS() As UShort
        Get
            Return Me.cOLSField
        End Get
        Set
            Me.cOLSField = value
            Me.RaisePropertyChanged("COLS")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property ROWS() As UShort
        Get
            Return Me.rOWSField
        End Get
        Set
            Me.rOWSField = value
            Me.RaisePropertyChanged("ROWS")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property LONGITUDESTART() As Decimal
        Get
            Return Me.lONGITUDESTARTField
        End Get
        Set
            Me.lONGITUDESTARTField = value
            Me.RaisePropertyChanged("LONGITUDESTART")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property LATITUDESTART() As Decimal
        Get
            Return Me.lATITUDESTARTField
        End Get
        Set
            Me.lATITUDESTARTField = value
            Me.RaisePropertyChanged("LATITUDESTART")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property DX() As Decimal
        Get
            Return Me.dxField
        End Get
        Set
            Me.dxField = value
            Me.RaisePropertyChanged("DX")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property DY() As Decimal
        Get
            Return Me.dyField
        End Get
        Set
            Me.dyField = value
            Me.RaisePropertyChanged("DY")
        End Set
    End Property
    
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    
    Protected Sub RaisePropertyChanged(ByVal propertyName As String)
        Dim propertyChanged As System.ComponentModel.PropertyChangedEventHandler = Me.PropertyChangedEvent
        If (Not (propertyChanged) Is Nothing) Then
            propertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyName))
        End If
    End Sub
End Class

'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038"),  _
 System.SerializableAttribute(),  _
 System.Diagnostics.DebuggerStepThroughAttribute(),  _
 System.ComponentModel.DesignerCategoryAttribute("code"),  _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=true)>  _
Partial Public Class PREVISIONSPREVISION
    Inherits Object
    Implements System.ComponentModel.INotifyPropertyChanged
    
    Private mField() As PREVISIONSPREVISIONM
    
    Private pREVISIONField As Byte
    
    Private dATEField As String
    
    '''<remarks/>
    <System.Xml.Serialization.XmlElementAttribute("M")>  _
    Public Property M() As PREVISIONSPREVISIONM()
        Get
            Return Me.mField
        End Get
        Set
            Me.mField = value
            Me.RaisePropertyChanged("M")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property PREVISION() As Byte
        Get
            Return Me.pREVISIONField
        End Get
        Set
            Me.pREVISIONField = value
            Me.RaisePropertyChanged("PREVISION")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property [DATE]() As String
        Get
            Return Me.dATEField
        End Get
        Set
            Me.dATEField = value
            Me.RaisePropertyChanged("DATE")
        End Set
    End Property
    
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    
    Protected Sub RaisePropertyChanged(ByVal propertyName As String)
        Dim propertyChanged As System.ComponentModel.PropertyChangedEventHandler = Me.PropertyChangedEvent
        If (Not (propertyChanged) Is Nothing) Then
            propertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyName))
        End If
    End Sub
End Class

'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038"),  _
 System.SerializableAttribute(),  _
 System.Diagnostics.DebuggerStepThroughAttribute(),  _
 System.ComponentModel.DesignerCategoryAttribute("code"),  _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=true)>  _
Partial Public Class PREVISIONSPREVISIONM
    Inherits Object
    Implements System.ComponentModel.INotifyPropertyChanged
    
    Private lATField As Decimal
    
    Private lONField As Decimal
    
    Private vField As Decimal
    
    Private dField As Short
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property LAT() As Decimal
        Get
            Return Me.lATField
        End Get
        Set
            Me.lATField = value
            Me.RaisePropertyChanged("LAT")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property LON() As Decimal
        Get
            Return Me.lONField
        End Get
        Set
            Me.lONField = value
            Me.RaisePropertyChanged("LON")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property V() As Decimal
        Get
            Return Me.vField
        End Get
        Set
            Me.vField = value
            Me.RaisePropertyChanged("V")
        End Set
    End Property
    
    '''<remarks/>
    <System.Xml.Serialization.XmlAttributeAttribute()>  _
    Public Property D() As Short
        Get
            Return Me.dField
        End Get
        Set
            Me.dField = value
            Me.RaisePropertyChanged("D")
        End Set
    End Property
    
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    
    Protected Sub RaisePropertyChanged(ByVal propertyName As String)
        Dim propertyChanged As System.ComponentModel.PropertyChangedEventHandler = Me.PropertyChangedEvent
        If (Not (propertyChanged) Is Nothing) Then
            propertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyName))
        End If
    End Sub
End Class
