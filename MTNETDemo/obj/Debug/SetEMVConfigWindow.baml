   M S B A M L   `   `   `  ����E  AMTNETDemo, Version=1.21.1.0, Culture=neutral, PublicKeyToken=null%clr-namespace:MTNETDemo	MTNETDemo  5   	   "    MTNETDemo.SetEMVConfigWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:MTNETDemo     5   	     � Title$  Set EMV Configuration��5   	   $	��600��6'   $	��340��64    � WindowStartupLocation$ CenterOwner=�6@   .��5	      � $��40,10,40,10q�6    � RowDefinitions 5
   
   �� 5      $��80 �6   �� 5      $��50 �6   �� 5      $	��110 �6   �� 5      $��60 �6   �� 5      $	��150 �6   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
   �� $��0��6   .+�5      �� $��	Category:��6   $
��Bold"�6(   �� 5      $��20,0,0,0q�6   .+�5      	� -   5        $��
TerminalRB��5       	� 	GroupName$ EMVConfigRB��60   $��Terminal��6H    d� 	IsChecked$
 TrueY�6[   	� 5      -   5        $��ApplicationRB��5      $ EMVConfigRB��63   $��Application��6K   	� 5      -   5        $��CapkRB��5      $ EMVConfigRB��6,   $
��CAPK��6D   5      5      �� 5   
   $��1��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� $��Slot:��6   $��Center=�6$   $
��Bold"�6F   �� 5      -   5         ��Name$ SlotTextBox��5       �� TextWrapping$
 Wrap=�6+   $��1��6?   $��50��6H   $��Center=�6S   $��21��6u   5       �� 5"   
   $��2��6   .+�5#      �� $��	Database:��6   $
��Bold"�6(   �� 5$      $��20,0,0,0q�6   .+�5%      	� -   5        $��Db0RB��5%      $ EMVDatabaseRB��6+   $��EMV L2 Contact��6E   $
 TrueY�6^   	� 5&      -   5        $��Db1RB��5&      $ EMVDatabaseRB��6+   $	��MCL��6E   	� 5'      -   5        $��Db2RB��5'      $ EMVDatabaseRB��6+   $��payWave��6E   	� 5(      -   5        $��Db3RB��5(      $ EMVDatabaseRB��6+   $��
Expresspay��6E   	� 5)      -	   5        $��Db4RB��5)      $ EMVDatabaseRB��6+   $��D-PAS��6E   5*      5+      �� 5-   
   $��3��6   $ Vertical=�6"   .+�5.      �� $&�� Device Serial Number (16 bytes):��6   $��Center=�6?   $
��Bold"�6a   �� 5/      -
   5        $ SerialTextBox��5/      $
 Wrap=�6-   $&�� 42333039343633303932323135414100��6A   $	��220��6i   $��Center=�6u   $��21��6�   $��16,0,0,0q�6�   $
��Left=�6�   50      �� 52   
   $��4��6   $ Vertical=�6"   .+�53      �� $��TLV Object Data:��6   $��Center=�6/   $
��Bold"�6Q   �� 54      -   5        $ DataTextBox��54      $
 Wrap=�6+   $�� ��6?   $��	20,0,20,0q�6G   $	��120��6Z   " �� VerticalScrollBarVisibility$
 Auto=�6g   	 �� AcceptsReturn	 . 6�   55      �� 57   
   $��5��6   $ 
Horizontal=�6"   .+�58      �� $��MAC (4 bytes):��6   $��Center=�6-   $
��Bold"�6O   �� 59      -   5        $ 
MACTextBox��59      $
 Wrap=�6*   $��CCDDEEFF��6>   $	��140��6N   $��Center=�6Z   $��21��6|   5:      �� 5<   
   $��6��6   $ 
Horizontal=�6"   .+�5=      �� -   5        
 ��Name$
 OKButton��5=      $��OK��6'   $	��100��64   $��30��6@   $��20,0,0,0q�6L   6^   �� 5>      -   5        $
 CancelButton��5>      $��Cancel��6+   $	��100��6<   $��30��6H   $��20,0,0,0q�6T    �� IsCancel . 6v   5?      5A      5B      