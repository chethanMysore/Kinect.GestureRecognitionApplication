   M S B A M L   `   `   `  ����  DiscreteGestureBasics-WPF?    9Microsoft.Samples.Kinect.DiscreteGestureBasics.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Kinect v2 Mouse Control��5   	   $	��450��6)   $��458.276��66    � 
ResizeMode$ CanMinimize=�6F    � WindowStartupLocation$ CenterScreen=�6_   .��5      � .+�5      ]� $��0��6   $��	0 0 0 370q�6   .+�5      �� $��1��6   $��Images\Status.png��6   $
��None=�69   $��Center=�6H   $��0 0 0 5q�6e   5	      � 5
      -   5        $��0,33,0,0q�5
   1   .+�5      �� -   5         ��Name$ MouseSensitivity��5      $��10,40,9.5,0q�6)   $	��Top=�6>   $��26��6V    � Maximum$ 6Q�6b    � Minimum$ 1Q�6�    � Value$	 3.5Q�6�   �� 5      $��Mouse Sensitivity��6   $��10,10,117.5,0q�6*   $	��Top=�6A   �� 5      -   5        $ PauseToClickTime��5      $��10,111,9.5,0q�6)   $	��Top=�6?   $��26��6W   $ 5Q�6c    � SmallChange$
 0.01Q�6o   $ 2Q�6�   $	 0.3Q�6�   �� 5      $"��Pause-To-Click Time Required��6   $��10,81,117.5,0q�65   $	��Top=�6L   �� 5      -   5         ��Name$ txtTimeRequired��5      $��23��6)   $��0,81,20.5,0q�65   	 �� TextWrapping$
	 Wrap=�6J   $	��Top=�6^   $��2��6v   $��15(�6   
 �� TextAlignment$
 Right=�6�   $��Right=�6�   $��77��6�   6�   �� 5      -   5        $ txtMouseSensitivity��5      $��23��6-   $��0,12,20.5,0q�69   $
	 Wrap=�6N   $	��Top=�6b   $	��3.5��6z   $��15(�6�   $
 Right=�6�   $��Right=�6�   $��77��6�   �� 5      -   5        $ PauseThresold��5      $��10,182,9.5,0q�6&   $	��Top=�6<   $��26��6T   $	 160Q�6`   $
 0.01Q�6n   $ 60Q�6�   $ 10Q�6�   �� 5      $��Pause Movement Thresold��6   $��10,152,117.5,0q�60   $	��Top=�6H   �� 5      -   5        $ txtPauseThresold��5      $��23��6*   $��0,152,20.5,0q�66   $
	 Wrap=�6L   $	��Top=�6`   $��60��6x   $��15(�6�   $
 Right=�6�   $��Right=�6�   $��77��6�   6�   �� 5      -	   5         ��Name$ 
btnDefault��5      $��Default��6#   $��Right=�65   $��0,323,15.5,0q�6Q   $��74��6g   $��24��6�   $	��Top=�6�   �� 5      -
   5         ��Name$ 
chkNoClick��5      $!��No clicks, move cursor only��6%   $
��Left=�6K   $��260,295,0,0q�6f   $��16��6{   $	��Top=�6�   6�   �� 5      -   5        $ CursorSmoothing��5      $��10,253,9.5,0q�6(   $	��Top=�6>   $��26��6V   $
 0.95Q�6b   $
 0.01Q�6q   $	 0.2Q�6�   6�   �� 5      $��Cursor Smoothing��6   $��10,223,117.5,0q�6)   $	��Top=�6A   �� 5      -   5        $ txtCursorSmoothing��5      $��23��6,   $��0,223,20.5,0q�68   $
	 Wrap=�6N   $	��Top=�6b   $	��0.2��6z   $��15(�6�   $
 Right=�6�   $��Right=�6�   $��77��6�   6�   5      5      5      