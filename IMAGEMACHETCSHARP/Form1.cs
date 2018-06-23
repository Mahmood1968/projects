using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;



namespace IMAGEMACHETCSHARP
{
    public partial class Form1 : Form
    {
        double Maxzncc = 0;
        int X1, Y1, C, V, tempx, temp2, matchx, matchy, pointcounter = 0; 
        double[,] firstimage = new double[576, 384];
        double[,] secondimage = new double[576, 384];
        double[,] window1 = new double[9, 9];
        public double[,] window2 = new double[49, 49];
        public Bitmap LeftImage = null;
        public Bitmap RightImage = null;
       // double[,] tempwindow = new double[9, 9];
        List<int> listx0 = new List<int>(); List<int> listy0 = new List<int>();
        List<int> listx1 = new List<int>(); List<int> listy1 = new List<int>();
        List<double> listzncc = new List<double>();

        public int[,] mpointL = new int[12, 2];
        public int[,] mpointR = new int[12, 2];
        double Z=0, Mr=0;
        public double[,] swindow = new double[9, 9];  //SEARCH WINDOW AT THE CORENER 
        
        //#########   SVD            SVD              SVD PARAMETERS ########## 
        public float[,]  a = new float[12, 9];
        public float[]   w = new float[9];
        public float[,]  v = new float[9, 9]; 
        public int          m=12,n=9 ;

        //#################   MEAN METHODS    MEAN METHODS    ################## 
        public double WindowsMean(double[,] W, int w)
        {

            double Mean = 0;
            for (int i = 0; i < w; i++)
                for (int j = 0; j < w; j++)
                    Mean = +W[i, j];
            return Mean / (w * w);

        }
        //###################################
        public void catsearchwindow(int lx, int ly)
        {

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    swindow[i, j] = this.RightImage.GetPixel(lx, ly).R;  // passed to RW right image 
                    lx++;
                }

                ly++;
            }

        }
        // ###############  ZNCC  METHODS  HERE  ############### ############### 
        public double ZNCC(double[,] RW, double[,] LW, int w)
        {
            double Numerator = 0, LWDenominator = 0, RWDenominator = 0,
                LWMean = WindowsMean(LW, w),
                RWMean = Mr; // = WindowsMean(RW, w);

            for (int i = 0; i < w; i++)
                for (int j = 0; j < w; j++)

                    Numerator += (RW[i, j] - RWMean) * (LW[i, j] - LWMean);

            for (int i = 0; i < w; i++)
                for (int j = 0; j < w; j++)
                {
                    LWDenominator += (LW[i, j] - LWMean) * (LW[i, j] - LWMean);
                    RWDenominator += (RW[i, j] - RWMean) * (RW[i, j] - RWMean);

                }
            return Numerator / System.Math.Sqrt(LWDenominator * RWDenominator);



        }
    //######################## END OF ZNCC ################################

    // #######################   SVD METHOD ###############################

//static double at,bt,ct;
static double PYTHAG(double a, double b)
{
    double at = Math.Abs (a), bt = Math.Abs (b), ct, result;

    if (at > bt) { 
        ct = bt / at; 
        result = at * Math.Sqrt (1.0 + ct * ct); 
         }
    else if (bt > 0.0) 
    { 
        ct = at / bt; 
        result = bt * Math.Sqrt (1.0 + ct * ct); 
    }
    else result = 0.0;
    return (result);
}

//############################################################

public double MAX(double a, double b)
{
    if (a > b) return a;
    else return b;
}
 
//############################################################
public double  SIGN(double a,double b) 
{
    if(b >= 0.0 ) b= Math.Abs(a);
              else b= -Math.Abs (a); 
    return b ; 
}


//############################################################

public int  dsvd()    // ( double  a, int m, int n, float w, float v)
{
    int flag=0, i=0, its, j, jj, k, l=0, nm=0;
    double c, f, h, s, x, y, z;
    double anorm = 0.0, g = 0.0, scale = 0.0;
    double []rv1=new double[9 ] ; 

    if (m < n)
    {
        Console.WriteLine ("#rows must be > #cols \n");
        return(0);
    }

   // rv1 = (double *)malloc((unsigned int) n*sizeof(double));

/* Householder reduction to bidiagonal form */
    for (i = 0; i < n; i++)
    {
        /* left-hand reduction */
        l = i + 1;
        rv1[i] = scale * g;
        g = s = scale = 0.0;
        if (i < m)
        {
            for (k = i; k < m; k++)
                scale += Math.Abs ((double)a[k,i]);
            if (scale!=0 )
            {
                for (k = i; k < m; k++)
                {
                    a[k,i] = (float)((double)a[k,i]/scale);
                    s += ((double)a[k,i] * (double)a[k, i]);
                }
                f = (double)a[i,i];
                g = -SIGN(Math.Sqrt (s), f);
                h = f * g - s;
                a[i,i] = (float)(f - g);
                if (i != n - 1)
                {
                    for (j = l; j < n; j++)
                    {
                        for (s = 0.0, k = i; k < m; k++)
                            s += ((double)a[k,i] * (double)a[k,j]);
                        f = s / h;
                        for (k = i; k < m; k++)
                            a[k,j] += (float)(f * (double)a[k,i]);
                    }
                }
                for (k = i; k < m; k++)
                    a[k,i] = (float)((double)a[k,i]*scale);
            }
        }
        w[i] = (float)(scale * g);

        /* right-hand reduction */
        g = s = scale = 0.0;
        if (i < m && i != n - 1)
        {
            for (k = l; k < n; k++)
                scale += Math.Abs ((double)a[i,k]);
            if (scale!=0)
            {
                for (k = l; k < n; k++)
                {
                    a[i,k] = (float)((double)a[i,k]/scale);
                    s += ((double)a[i,k] * (double)a[i,k]);
                }
                f = (double)a[i,l];
                g = -SIGN(Math.Sqrt (s), f);
                h = f * g - s;
                a[i,l] = (float)(f - g);
                for (k = l; k < n; k++)
                    rv1[k] = (double)a[i,k] / h;
                if (i != m - 1)
                {
                    for (j = l; j < m; j++)
                    {
                        for (s = 0.0, k = l; k < n; k++)
                            s += ((double)a[j,k] * (double)a[i,k]);
                        for (k = l; k < n; k++)
                            a[j,k] += (float)(s * rv1[k]);
                    }
                }
                for (k = l; k < n; k++)
                    a[i,k] = (float)((double)a[i,k]*scale);
            }
        }
        anorm = MAX(anorm, (Math.Abs ((double)w[i]) + Math.Abs (rv1[i])));
    }
   // l = i;
    /* accumulate the right-hand transformation */
    for (i = n - 1; i >= 0; i--)
    {
        if (i < n - 1)
        {
            if (g>0)
            {
                for (j = l; j < n; j++)
                    v[j,i] = (float)(((double)a[i,j] / (double)a[i,l]) / g);
                    /* double division to avoid underflow */
                for (j = l; j < n; j++)
                {
                    for (s = 0.0, k = l; k < n; k++)
                        s += ((double)a[i,k] * (double)v[k,j]);
                    for (k = l; k < n; k++)
                        v[k,j] += (float)(s * (double)v[k,i]);
                }
            }
            for (j = l; j < n; j++)
                v[i,j] = v[j,i] = 0;
        }
        v[i,i] = 1;
        g = rv1[i];
        l = i;
    }

    /* accumulate the left-hand transformation */
    for (i = n - 1; i >= 0; i--)
    {
        l = i + 1;
        g = (double)w[i];
        if (i < n - 1)
            for (j = l; j < n; j++)
                a[i,j] = 0;
        if (g>0)
        {
            g = 1.0 / g;
            if (i != n - 1)
            {
                for (j = l; j < n; j++)
                {
                    for (s = 0.0, k = l; k < m; k++)
                        s += ((double)a[k,i] * (double)a[k,j]);
                    f = (s / (double)a[i,i]) * g;
                    for (k = i; k < m; k++)
                        a[k,j] += (float)(f * (double)a[k,i]);
                }
            }
            for (j = i; j < m; j++)
                a[j,i] = (float)((double)a[j,i]*g);
        }
        else
        {
            for (j = i; j < m; j++)
                a[j,i] = 0;
        }
        ++a[i,i];
    }

    /* diagonalize the bidiagonal form */
    for (k = n - 1; k >= 0; k--)
    {                                    /* loop over singular values */
        for (its = 0; its < 30; its++)
        {                                /* loop over allowed iterations */
            flag = 1;
            for (l = k; l >= 0; l--)
            {                     /* test for splitting */
                nm = l - 1;
                if (Math.Abs (rv1[l]) + anorm == anorm)
                {
                    flag = 0;
                    break;
                }
                if (Math.Abs ((double)w[nm]) + anorm == anorm)
                    break;
            }
            if (flag!=0)
            {
                c = 0.0;
                s = 1.0;
                for (i = l; i <= k; i++)
                {
                    f = s * rv1[i];
                    if (Math.Abs(f) + anorm != anorm)
                    {
                        g = (double)w[i];
                        h = PYTHAG(f, g);
                        w[i] = (float)h;
                        h = 1.0 / h;
                        c = g * h;
                        s = (- f * h);
                        for (j = 0; j < m; j++)
                        {
                            y = (double)a[j,nm];
                            z = (double)a[j,i];
                            a[j,nm] = (float)(y * c + z * s);
                            a[j,i] = (float)(z * c - y * s);
                        }
                    }
                }
            }
            z = (double)w[k];
            if (l == k)
            {                  /* convergence */
                if (z < 0.0)
                {              /* make singular value nonnegative */
                    w[k] = (float)(-z);
                    for (j = 0; j < n; j++)
                        v[j,k] = (-v[j,k]);
                }
                break;
            }
            if (its >= 30) {
               // free((void*) rv1);
                Console.WriteLine ("No convergence after 30,000! iterations \n");
                return(0);
            }

            /* shift from bottom 2 x 2 minor */
            x = (double)w[l];
            nm = k - 1;
            y = (double)w[nm];
            g = rv1[nm];
            h = rv1[k];
            f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
            g = PYTHAG(f, 1.0);
            f = ((x - z) * (x + z) + h * ((y / (f + SIGN(g, f))) - h)) / x;

            /* next QR transformation */
            c = s = 1.0;
            for (j = l; j <= nm; j++)
            {
                i = j + 1;
                g = rv1[i];
                y = (double)w[i];
                h = s * g;
                g = c * g;
                z = PYTHAG(f, h);
                rv1[j] = z;
                c = f / z;
                s = h / z;
                f = x * c + g * s;
                g = g * c - x * s;
                h = y * s;
                y = y * c;
                for (jj = 0; jj < n; jj++)
                {
                    x = (double)v[jj,j];
                    z = (double)v[jj,i];
                    v[jj,j] = (float)(x * c + z * s);
                    v[jj,i] = (float)(z * c - x * s);
                }
                z = PYTHAG(f, h);
                w[j] = (float)z;
                if (z!=0)
                {
                    z = 1.0 / z;
                    c = f * z;
                    s = h * z;
                }
                f = (c * g) + (s * y);
                x = (c * y) - (s * g);
                for (jj = 0; jj < m; jj++)
                {
                    y = (double)a[jj,j];
                    z = (double)a[jj,i];
                    a[jj,j] = (float)(y * c + z * s);
                    a[jj,i] = (float)(z * c - y * s);
                }
            }
            rv1[l] = 0.0;
            rv1[k] = f;
            w[k] = (float)x;
        }
    }
    //free((void*) rv1);
    return(1);
}



        



    //#####################  END OF SVD METHOD HERE ###########################



        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Size = SystemInformation.PrimaryMonitorSize   ;
            // this.Size(1200, 1200); 

        }

        //#############################  LOADING IMAGE LEFT ONE  ###########################################
        public void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = new Bitmap(open.FileName);
                    pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Failed loading image");
            }
            LeftImage = (Bitmap)pictureBox1.Image;


        }
        //#################################LOADING RIGHT IMAGE ####################################################
        public void button2_Click(object sender, EventArgs e)
        {
            // THIS PART LOAD ANY IMAGE IN THIS PICTURE BOX 
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image = new Bitmap(open.FileName);
                    pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Failed loading image");
            }
            RightImage = (Bitmap)pictureBox2.Image;

        }


        //#####################################################################################
        private void button3_Click(object sender, EventArgs e)
        {

            this.Close();


        }

        //#####################################################################################     

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            V = e.X;
            C = e.Y;
            int maxy = LeftImage.Height;
            int maxx = LeftImage.Width
                   , k, l, maxw = 9;

            tempx = V;
            temp2 = C;

            textBox1.Text = V.ToString();
            textBox2.Text = C.ToString();
            LImgIntensity.Text = LeftImage.GetPixel(V, C).R.ToString();
            Graphics g = pictureBox2.CreateGraphics();
                                    // THIS PART FOR LEFT IMAGE WINDOW 1 IT IS 9 BY 9 
            int border1 = 9;
            if (V >= maxx - 9 || C >= maxy - 9 || V < border1 || C < border1)
            {
                MessageBox.Show("Click away from the border ", "Out range  Error",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                k = V - 4; l = C - 4;
                for (int i = 0; i < maxw; i++)
                {
                    k = V - 4;
                    for (int j = 0; j < maxw; j++)
                    {
                        window1[i, j] = LeftImage.GetPixel(k, l).R;   // CRAET WINDOW 1 FROM THE LEFT IMAGE 
                        k++;
                    }
                    l++;
                }
            }
            Mr = WindowsMean(window1, maxw);
        //    Console.WriteLine("Mr is {0} ", Mr);

            g.Dispose();
        }
        // THIS PRINTING OUT 
        /*     Console.WriteLine(" THE WINDOW 1 IS PRINTED HERE FOR VARIFICATION \n"); 
             for( int i=0 ;i<=8 ;i++){

                for (int j=0 ; j<=8 ;j++) 
                    Console.Write( window1 [i,j]+"  " )  ;
                    Console.WriteLine(); 
                     } 
               
                }        
  */

                                                  // THIS DEECT BUTTON HERE 
        private void button4_Click(object sender, EventArgs e)
        {
            //  double Maxzncc=0  , matchx , matchy;
            X1 = V + 40;             Y1 = C - 10;
            int maxx, maxy;
            maxx = RightImage.Width;             maxy = RightImage.Height;
            int border2 = 24, k, l; ;
            Z = 0; Maxzncc = 0;
            Graphics f;
            f = pictureBox2.CreateGraphics();   f.DrawEllipse(Pens.Blue, X1, Y1, 20, 20);
            f.DrawRectangle(Pens.Blue, X1 - 10, Y1 - 10, 40, 40);

            this.textBox3.Text = X1.ToString();   
            this.textBox4.Text = Y1.ToString();
            this.LIMGEPIXEL.Text = this.RightImage.GetPixel(X1, Y1).R.ToString();

            //THIS WINDOW IS 49 BY 49   
            if (X1 >= maxx - 24 || Y1 >= maxy - 24 || X1 < border2 || Y1 < border2)
            {
                MessageBox.Show("Click away from the border ", "Out range  Error",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            else
            {
                k = X1 - 24;         l = Y1 - 24;
                for (int i = 0; i <= 48; i++)
                {
                    k = V - 24;
                    for (int j = 0; j <= 48; j++)
                    {
                     //   window2[i, j] = RightImage.GetPixel(k, l).R;
                        k++;
                        catsearchwindow(k, l);      // THIS IS THE PATRIAL BOX FROM SEARCH AREA > > > 
                        Z = ZNCC(window1, swindow, 9);//  swindow comes from catsearchwindow   .. 
                        if (Z > Maxzncc)
                        {
                            Maxzncc = Z;
                            matchx = k; matchy = l;
                             
                        }
                //   Console.WriteLine (" THE ZNCC IS {0} ",Z ) ; 
                    }
                    l++;
                   // if (Z >= 0.8700000 ) break; 
                }
            }

            this.textBoxZNCC.Text = Maxzncc.ToString();

            f.Dispose(); 

        }



        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            X1 = e.X; Y1 = e.Y;
            textBox3.Text = X1.ToString();
            textBox4.Text = Y1.ToString();

        }

        private void ACCEPT_Click(object sender, EventArgs e)
        {

            this.textBox3.Text = matchx.ToString();
            this.textBox4.Text = matchy.ToString();
            this.LIMGEPIXEL.Text = this.RightImage.GetPixel(matchx, matchy).R.ToString();
            listx0.Add(V);
            listy0.Add(C);
            listx1.Add(matchx);
            listy1.Add(matchy);
            listzncc.Add(Maxzncc);
            
            pointcounter++;
            this.textBox5Counter.Text = pointcounter.ToString(); 

        }
        //   FM FM  FM 
        private void button5_Click(object sender, EventArgs e)
        {
        //    int [,] A=new int [12,9 ] ;
            if (pointcounter < 12)
            {

                MessageBox.Show("Number of point should be 12  ", "Number of Points ",
               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
             }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    a[i,0]=listx1[i]*listx0 [i];
                    a[i,1]=listx1[i]*listy0 [i]; 
                    a[i,2]=listx1 [i];
                    a[i,3]=listy1 [i]*listx0 [i]; 
                    a[i,4]=listy1 [i]*listy0 [i];
                    a[i,5]=listy1 [i];
                    a[i,6]=listx0 [i]; 
                    a[i,7]=listy0 [i];
                    a[i,8] =1; 

                   
                }
                Console.WriteLine (" THE MATCHED POINTS ARE ") ; 
                for (int i=0;i<12 ;i++) 
                {
                  Console.Write  ("{0} ", listx0[i]);
                  Console.Write("{0} ", listy0[i]);
                  Console.Write("{0}  ", listx1[i]);
                  Console.WriteLine ("{0}  ", listy1[i]);
                }
             //   dsvd(); 
                Console.WriteLine (" THE A MATRIX IS :  ") ; 
              
                    for (int i = 0; i < 12; i++)
                    {
                        for(int j=0 ;j< 9 ; j++) 
                        {
                            
                            Console.Write(" {0}   ", a[i, j]);
                          }
                    Console.WriteLine();
                }

                    dsvd(); 
              Console.WriteLine(" THE V  VECTOR  ARE ");
              for (int i = 0; i < 9; i++)
              {
                  for (int j = 0; j < 9; j++)
                  {
                      Console.Write("{0}  ",v[i,j]); 
              
                  }
                  Console.WriteLine(); 
              
              }

              Console.WriteLine(" THE W eginvalues  ");
              for (int i = 0; i < 9; i++)
              {
                 
                      
                  Console.WriteLine("{0}  ", w[i]);; 

              }
                      
            }

            float [,] FM = new float [3, 3];
            int p = 0; 
            for (int i=0 ; i<3; i++)
            {      
                for (int j = 0; j < 3; j++)
                {
                   
                   FM[i, j] = v[p, 8];
                   p++;
                   
                }
            }
            Console.WriteLine(" THE F MATRIX   ");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write("{0}  ", FM[i, j]);

                }
                Console.WriteLine();

            }


        }
    }
}
