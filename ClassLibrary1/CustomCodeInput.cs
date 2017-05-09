using System;
using Android.Views;
using Android.Animation;
using Android.Graphics;
using Java.Lang;
using Android.Util;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views.InputMethods;
using System.Linq;
using Android.Views.Animations;
using Java.Util.Regex;
using Android.OS;

namespace ClassLibrary1
{
    public class CustomCodeInput : View
    {
        //static const
        private const int DEFAULT_CODES = 6;
        //static const
        private const string KEYCODE = "KEYCODE_";
        public Character chracter;
        private FixedStack<Character> characters;
        private static Java.Util.Regex.Pattern KEYCODE_PATTERN = Java.Util.Regex.Pattern.Compile(KEYCODE + "(\\w" + ")");
        private Underline[] underlines;
        private Paint underlinePaint;
        private Paint underlineSelectedPaint;
        private Paint textPaint;
        private static  Paint hintPaint;
        private ValueAnimator reductionAnimator;
        private ValueAnimator hintYAnimator;
        private ValueAnimator hintSizeAnimator;
        private float underlineReduction;
        private float underlineStrokeWidth;
        private float underlineWidth;
        private float reduction;
        private float textSize;
        private float textMarginBottom;
        private float hintX;
        private float hintNormalSize;
        private float hintSmallSize;
        private float hintMarginBottom;
        public  static float hintActualMarginBottom;
        private float viewHeight;
        private long animationDuration;
        private int height;
        private int underlineAmount;
        private int underlineColor;
        private int underlineSelectedColor;
        private int hintColor;
        private int textColor;
        private bool underlined = true;
        private string hintText;
        private int mInputType;
        private codeReadyListener listener;
        //Constructors
        protected CustomCodeInput(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
       public CustomCodeInput(Context context) : base(context)
        {
            init(null);
            this.listener = null;
        }   
        public CustomCodeInput(Context context, IAttributeSet attributeset) : base(context, attributeset)
        {
            init(attributeset);
            this.listener = null;
        }
        public CustomCodeInput(Context context, IAttributeSet attributeset, int defStyledAttrs) : base(context, attributeset, defStyledAttrs)
        {
            init(attributeset);
            this.listener = null;
        }
        //End of Constructors 
        public void setCodeReadyListener(codeReadyListener listener)
        { 
            this.listener = listener;
        }
        private void init(IAttributeSet at)
        {
            initDefaultAttributes();
            initCustomAttributes(at);
            initDataStructures();
            initPaint();
            initAnimator();
            initViewOptions();
        }
        public interface codeReadyListener
        {
            // These methods are the different events and
            // need to pass relevant arguments related to the event triggered
            void onCodeReady(Character[] code); 
        }
        private void initDefaultAttributes()
        {
            hintMarginBottom = Context.Resources.GetDimension(Resource.Dimension.hint_margin_bottom);
            underlineStrokeWidth = Context.Resources.GetDimension(Resource.Dimension.underline_stroke_width);
            underlineWidth = Context.Resources.GetDimension(Resource.Dimension.underline_width);
            underlineReduction = Context.Resources.GetDimension(Resource.Dimension.section_reduction);
            textSize = Context.Resources.GetDimension(Resource.Dimension.text_size);
            textMarginBottom = Context.Resources.GetDimension(Resource.Dimension.text_margin_bottom);
            underlineColor = (Resource.Color.underline_default_color);
            underlineSelectedColor = (Resource.Color.underline_selected_color);
            hintColor = (Resource.Color.hintColor);
            textColor = (Resource.Color.textColor);
            hintMarginBottom = Context.Resources.GetDimension(Resource.Color.underline_default_color);
            hintNormalSize = Context.Resources.GetDimension(Resource.Dimension.hint_size);
            hintSmallSize = Context.Resources.GetDimension(Resource.Dimension.hint_small_size);
            animationDuration = Context.Resources.GetInteger(Resource.Color.underline_default_color);
            viewHeight = Context.Resources.GetDimension(Resource.Dimension.view_height);
            hintX = 0;
            hintActualMarginBottom = 0;
            underlineAmount = DEFAULT_CODES;
            reduction = 0.0F;
            hintX = 0;
            hintActualMarginBottom = 0;
            underlineAmount = DEFAULT_CODES;
            reduction = 0.0F;
        }
        private void initCustomAttributes(IAttributeSet attributeset)
        {
            TypedArray attributes = Context.ObtainStyledAttributes(attributeset, Resource.Styleable.core_area);
            underlineColor = attributes.GetColor(Resource.Styleable.core_area_underline_color, underlineColor);
            underlineSelectedColor = attributes.GetColor(Resource.Styleable.core_area_underline_selected_color, underlineSelectedColor);
            hintColor = attributes.GetColor(Resource.Styleable.core_area_hint_color, hintColor);
            hintText = attributes.GetString(Resource.Styleable.core_area_hint_text);
            underlineAmount = attributes.GetInt(Resource.Styleable.core_area_codes, underlineAmount);
            textColor = attributes.GetInt(Resource.Styleable.core_area_text_color, textColor);
            attributes.Recycle();
        }
        private void initDataStructures()
        {
            underlines = new Underline[underlineAmount];
            characters = new FixedStack<Character>();
            characters.setMaxSize(underlineAmount);
        }
        public void initPaint()
        {
            underlinePaint = new Paint();
            // underlinePaint.C
            underlinePaint.Set((Paint)Resource.Attribute.underline_color);
            underlinePaint.StrokeWidth= underlineStrokeWidth;
            underlinePaint.SetStyle(Paint.Style.Stroke);
            underlineSelectedPaint = new Paint();
            // underlineSelectedPaint.Color = Resources.GetColor(underlineSelectedColor);
            underlineSelectedPaint.Set((Paint)Resource.Color.underline_selected_color);
            underlineSelectedPaint.StrokeWidth=(underlineStrokeWidth);
            underlineSelectedPaint.SetStyle(Paint.Style.Stroke);
            textPaint = new Paint();
            textPaint.TextSize=textSize;
            // textPaint.Color=Resources.GetColor(Resource.Color.textColor);
            textPaint.Set((Paint)Resource.Color.textColor);
            textPaint.AntiAlias=true;
            textPaint.TextAlign=Paint.Align.Center;
            hintPaint = new Paint();
            hintPaint.TextSize=hintNormalSize;
            hintPaint.AntiAlias=true;
            // hintPaint.Color = Resources.GetColor(underlineColor);
            hintPaint.Set((Paint)Resource.Attribute.underline_color);
        }
        private void initAnimator()
        {
            reductionAnimator = ValueAnimator.OfFloat(0, underlineReduction);
            reductionAnimator.SetDuration(animationDuration);
            reductionAnimator.AddUpdateListener(new ReductionAnimatorListener());
            reductionAnimator.SetInterpolator(new AccelerateDecelerateInterpolator());
            hintSizeAnimator = ValueAnimator.OfFloat(hintNormalSize, hintSmallSize);
            hintSizeAnimator.SetDuration(animationDuration);
            hintSizeAnimator.AddUpdateListener(new HintSizeAnimatorListener());
            hintSizeAnimator.SetInterpolator(new AccelerateDecelerateInterpolator());
            hintYAnimator = ValueAnimator.OfFloat(0, hintMarginBottom);
            hintYAnimator.SetDuration(animationDuration);
            hintYAnimator.AddUpdateListener(new HintYAnimatorListener());
            hintYAnimator.SetInterpolator(new AccelerateDecelerateInterpolator());
        }
        private void initViewOptions()
        {
            Focusable = true;
            FocusableInTouchMode = true;
        }
        protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
            if (!gainFocus && characters.ToArray().Length == 0)
            {
                reverseAnimation();
            }
        }
        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        { 
            base.OnSizeChanged( (int)((underlineWidth + underlineReduction) * DEFAULT_CODES), (int)viewHeight, oldw, oldh);
            height = h;
            initUnderline();
        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension((int)((underlineWidth + underlineReduction) * DEFAULT_CODES), (int)viewHeight);
            
        }
        private void initUnderline()
        {
            for (int i = 0; i < underlineAmount; i++)
            {
                underlines[i] = createPath(i, underlineWidth);
            }
        }
        private Underline createPath(int position, float sectionWidth)
        {
            float fromX = sectionWidth * (float)position;
            return new Underline(fromX, height, fromX + sectionWidth, height);
        }
        private void showKeyboard()
        {
            InputMethodManager inputmethodmanager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            inputmethodmanager.ShowSoftInput(this, (ShowFlags)InputMethodResults.UnchangedShown);
            inputmethodmanager.ViewClicked(this);
        }
        private void hideKeyBoard()
        {
            InputMethodManager imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            //Input Method manager Doesnt have UnchangedShown
            imm.HideSoftInputFromWindow(WindowToken,(HideSoftInputFlags)InputMethodResults.UnchangedShown);
        }
        public void setInputType(int inputType)
        {
            mInputType = inputType;
        }
        public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
        {
            outAttrs.ActionLabel = null;
            outAttrs.InputType = (Android.Text.InputTypes)mInputType;
            outAttrs.ImeOptions = (ImeFlags) ImeAction.Done;
            return new BaseInputConnection(this, true);
        }
        public override bool OnCheckIsTextEditor()
        {
            return true;
        }
        private void startAnimation()
        {
            reductionAnimator.Start();
            hintSizeAnimator.Start();
            hintYAnimator.Start();
            underlined = false;
        }
        private void reverseAnimation()
        {
            reductionAnimator.Reverse();
            hintSizeAnimator.Reverse();
            hintYAnimator.Reverse();
            underlined = true;
        }
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Del && characters.ToArray().Length != 0)
               // if (keyCode == KeyEvent.KEYCODE_DEL && characters.size() != 0)
                {
                characters.Pop();

                    }

            return base.OnKeyDown(keyCode, e);
        }
        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            string text = KeyEvent.KeyCodeToString(keyCode);
            return true;

        }
      
        public void setCode(string code) {
            if (underlined)
            {
                startAnimation();
            }
            Handler handler = new Handler();
            handler.PostDelayed(
                delegate {
                    StringBuilder stringBuilder;
                    while (characters.ToArray().Length >= code.ToCharArray().Length && characters.ToArray().Length != 0)
                    {
                        characters.Pop();
                    }
                    foreach (char c in code.ToCharArray())
                    {
                        stringBuilder = new StringBuilder(KEYCODE);
                        stringBuilder.Append(c);
                        inputText(Java.Lang.String.ValueOf(stringBuilder));
                    }
                    if (characters.ToArray().Length==0)
                    {
                        hideKeyBoard();
                        reverseAnimation();
                    }
                } , animationDuration);

        }
        private bool inputText(string text)
         {
             Matcher matcher = KEYCODE_PATTERN.Matcher(text);
             if (matcher.Matches()) {
                chracter =(Character)(matcher.Group(1).ToCharArray().ElementAt(0).ToString());
                 characters.Push(chracter);
                 if (characters.getMaxSize() >= underlineAmount)
                 {
                     if (listener != null)
                     {
                         listener.onCodeReady(getCode());
                     }
                 }
                 return true;
             }
             else
             {
                 return false;
             }
         }
        public override bool OnTouchEvent(MotionEvent motionevent)
        {
            if (motionevent.Action == 0)
            {
                RequestFocus();
                if (underlined)
                {
                    startAnimation();
                }
                showKeyboard();
                }
            return base.OnTouchEvent(motionevent);
        }
        protected override void OnDraw(Canvas canvas)
        {
            for (int i = 0; i < underlines.Length; i++)
            {
                Underline sectionpath = underlines[i];
                float fromX = sectionpath.getFromX() + reduction;
                float fromY = sectionpath.getFromY();
                float toX = sectionpath.getToX() - reduction;
                float toY = sectionpath.getToY();
                drawSection(i, fromX, fromY, toX, toY, canvas);
                if (characters.ToArray().Length > i && characters.ToArray().Length != 0)
                {
                drawCharacter(fromX, toX, characters.ElementAt(0), canvas);
                }
            }
            if (hintText != null)
            {
                drawHint(canvas);
            }
            Invalidate();
        }
        private void drawSection(int position, float fromX, float fromY, float toX, float toY, Canvas canvas)
        {
            Paint paint = underlinePaint;
            if (position == characters.getMaxSize() && !underlined)
            {
                paint = underlineSelectedPaint;
            }
            canvas.DrawLine(fromX, fromY, toX, toY, paint);
        }
        private void drawCharacter(float fromX, float toX, Character character, Canvas canvas)
        {
            float actualWidth = toX - fromX;
            float centerWidth = actualWidth / 2;
            float centerX = fromX + centerWidth;
            canvas.DrawText(character.ToString(), centerX, height - textMarginBottom, textPaint);
        }
        private void drawHint(Canvas canvas)
        {
            canvas.DrawText(hintText, hintX, height - textMarginBottom - hintActualMarginBottom, hintPaint);
        }
        public Character[] getCode()
        {
            Character[] ar = new Character [100];
            ar = characters.ToArray();
            return ar ;
        }
        private class ReductionAnimatorListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
           {
               private float reduction;
            public void OnAnimationUpdate(ValueAnimator animation)
            {
                //Take String Argument
                float value = ((Float)animation.AnimatedValue).FloatValue();
                reduction = value;
            }   
       }
        //End Of The First one 
        private class HintYAnimatorListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
           {
            public  void OnAnimationUpdate(ValueAnimator animation)
               {
                    hintActualMarginBottom = (float) animation.AnimatedValue;
               }
           }
        //End of The Second
        private class HintSizeAnimatorListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
        {
            void ValueAnimator.IAnimatorUpdateListener.OnAnimationUpdate(ValueAnimator animation)
            {
                float size = (float)animation.AnimatedValue;
                hintPaint.Set((Paint)size);
            }
        }
        // override Animationupdae //End of the 3 one .
    }
    }
