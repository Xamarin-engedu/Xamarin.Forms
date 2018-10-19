﻿using System;
using System.ComponentModel;
using Android.Content.Res;
using AView = Android.Views.View;
using Android.Graphics.Drawables;
using Android.OS;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AButton = Android.Widget.Button;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	internal class BorderBackgroundManager : IDisposable
	{
		Drawable _defaultDrawable;
		BorderDrawable _backgroundDrawable;
		RippleDrawable _rippleDrawable;
		bool _drawableEnabled;
		bool _disposed;
		IBorderVisualElementRenderer _renderer;
		VisualElement Element => _renderer?.Element;
		AView Control => _renderer?.View;
		readonly bool _drawOutlineWithBackground;

		public bool DrawOutlineWithBackground { get; set; } = true;

		public BorderBackgroundManager(IBorderVisualElementRenderer renderer) : this(renderer, true)
		{
		}

		public BorderBackgroundManager(IBorderVisualElementRenderer renderer, bool drawOutlineWithBackground)
		{
			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;
			_drawOutlineWithBackground = drawOutlineWithBackground;
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				(e.OldElement as IBorderController).PropertyChanged -= BorderElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				if (BorderElement != null)
				{
					BorderElement.PropertyChanged -= BorderElementPropertyChanged;
				}
				BorderElement = (IBorderController)e.NewElement;
				BorderElement.PropertyChanged += BorderElementPropertyChanged;
			}

			Reset();
			UpdateDrawable();
		}

		public IBorderController BorderElement
		{
			get;
			private set;
		}

		public void UpdateDrawable()
		{
			if (BorderElement == null || Control == null)
				return;

			bool cornerRadiusIsDefault = !BorderElement.IsSet(BorderElement.CornerRadiusProperty) || (BorderElement.CornerRadius == (int)BorderElement.CornerRadiusProperty.DefaultValue || BorderElement.CornerRadius == BorderDrawable.DefaultCornerRadius);
			bool backgroundColorIsDefault = !BorderElement.IsSet(VisualElement.BackgroundColorProperty) || BorderElement.BackgroundColor == (Color)VisualElement.BackgroundColorProperty.DefaultValue;
			bool borderColorIsDefault = !BorderElement.IsSet(BorderElement.BorderColorProperty) || BorderElement.BorderColor == (Color)BorderElement.BorderColorProperty.DefaultValue;
			bool borderWidthIsDefault = !BorderElement.IsSet(BorderElement.BorderWidthProperty) || BorderElement.BorderWidth == (double)BorderElement.BorderWidthProperty.DefaultValue;

			if (backgroundColorIsDefault
				&& cornerRadiusIsDefault
				&& borderColorIsDefault
				&& borderWidthIsDefault)
			{
				if (!_drawableEnabled)
					return;

				if (_defaultDrawable != null)
					Control.SetBackground(_defaultDrawable);

				_drawableEnabled = false;
			}
			else
			{
				if (_backgroundDrawable == null)
					_backgroundDrawable = new BorderDrawable(Control.Context.ToPixels, Forms.GetColorButtonNormal(Control.Context), _drawOutlineWithBackground);

				_backgroundDrawable.BorderController = BorderElement;

				var useDefaultPadding = _renderer.UseDefaultPadding();

				int paddingTop = useDefaultPadding ? Control.PaddingTop : 0;
				int paddingLeft = useDefaultPadding ? Control.PaddingLeft : 0;

				var useDefaultShadow = _renderer.UseDefaultShadow();

				// Use no shadow by default for API < 16
				float shadowRadius = 0;
				float shadowDy = 0;
				float shadowDx = 0;
				AColor shadowColor = Color.Transparent.ToAndroid();
				// Add Android's default material shadow if we want it
				if (useDefaultShadow)
				{
					shadowRadius = 2;
					shadowDy = 4;
					shadowDx = 0;
					shadowColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();
				}
				// Otherwise get values from the control (but only for supported APIs)
				else if ((int)Build.VERSION.SdkInt >= 16)
				{
					shadowRadius = _renderer.ShadowRadius;
					shadowDy = _renderer.ShadowDy;
					shadowDx = _renderer.ShadowDx;
					shadowColor = _renderer.ShadowColor;
				}

				if (_renderer.IsShadowEnabled())
				{
					_backgroundDrawable.SetShadow(shadowDy, shadowDx, shadowColor, shadowRadius);
				}

				if (_drawableEnabled)
					return;

				if (_defaultDrawable == null)
					_defaultDrawable = Control.Background;

				if (Forms.IsLollipopOrNewer)
				{
					var rippleColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();

					_rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), _backgroundDrawable, null);
					Control.SetBackground(_rippleDrawable);
				}
				else
				{
					Control.SetBackground(_backgroundDrawable);
				}

				_drawableEnabled = true;
			}

			Control.Invalidate();
		}

		public void Reset()
		{
			if (_drawableEnabled)
			{
				_drawableEnabled = false;
				_backgroundDrawable?.Reset();
				_backgroundDrawable = null;
				_rippleDrawable = null;
			}
		}

		public void UpdateBackgroundColor()
		{
			UpdateDrawable();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_backgroundDrawable?.Dispose();
					_backgroundDrawable = null;
					_defaultDrawable?.Dispose();
					_defaultDrawable = null;
					_rippleDrawable?.Dispose();
					_rippleDrawable = null;
					if (BorderElement != null)
					{
						BorderElement.PropertyChanged -= BorderElementPropertyChanged;
						BorderElement = null;
					}

					if (_renderer != null)
					{
						_renderer.ElementChanged -= OnElementChanged;
						_renderer = null;
					}
				}
				_disposed = true;
			}
		}

		void BorderElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals(BorderElement.BorderColorProperty.PropertyName) ||
				e.PropertyName.Equals(BorderElement.BorderWidthProperty.PropertyName) ||
				e.PropertyName.Equals(BorderElement.CornerRadiusProperty.PropertyName) ||
				e.PropertyName.Equals(VisualElement.BackgroundColorProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultPaddingProperty.PropertyName) ||
				e.PropertyName.Equals(Specifics.Button.UseDefaultShadowProperty.PropertyName))
			{
				Reset();
				UpdateDrawable();
			}
		}

	}
}