package com.zst.hp.snmp_android;

import android.app.Activity;
import android.content.Intent;
import android.support.v7.app.AlertDialog;
import android.support.v7.widget.AppCompatRadioButton;

import android.content.DialogInterface;
import android.support.v7.app.AppCompatActivity;
import android.graphics.drawable.BitmapDrawable;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.GestureDetector;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.PopupWindow;
import android.widget.TextView;
import android.widget.RadioButton;
import android.widget.RadioGroup;

import static com.zst.hp.snmp_android.R.id.editText;

public class Settings extends Activity {

    public EditText response;
    public  EditText send;
    public EditText editTextAddress, editTextPort;
    public Button buttonConnect;
    private GestureDetector gestureDetector;
    public static RadioGroup radio_g;
    public static RadioButton radio_b;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_settings);

        editTextAddress = (EditText) findViewById(R.id.editText);
        editTextPort = (EditText) findViewById(R.id.editText2);
        buttonConnect = (Button) findViewById(R.id.button);
        gestureDetector = new GestureDetector(new SwipeGestureDetector());
    }

    public void buttonOnClick(View v)
    {
        String ip;
        ip = editTextAddress.getText().toString();
        int port = Integer.parseInt(editTextPort.getText().toString());
        String portID;
        portID = editTextPort.getText().toString();
        String temp = ip;
        Log.e("A",""+portID);
        if (!portID.equals(""))
        {

            if (port == 1235 && ip == temp) {
                snmp.connect(ip, port);
                showAlert("Nawiązano połączenie z " + ip + " na porcie: " + port, "Połączono!");
            } else {
                Log.e("A", "Wprowadzono złe dane");
                showAlert("Wprowadzono złe dane. Aby się połączyć spróbuj jeszcze raz", "Błąd połączenia!");

            }
        }
        else
        {
                showAlert("Nie wprowadzono kompletnych danych", "Brak danych!");
        }
    }

    @Override
    public boolean onTouchEvent(MotionEvent event) {
        if (gestureDetector.onTouchEvent(event)) {
            return true;
        }
        return super.onTouchEvent(event);
    }

    private void onRightSwipe() {
        Intent intent = new Intent(Settings.this, snmp.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
        startActivity(intent);

    }

    private class SwipeGestureDetector extends GestureDetector.SimpleOnGestureListener {
        // Swipe properties, you can change it to make the swipe
        // longer or shorter and speed
        private static final int SWIPE_MIN_DISTANCE = 60;
        private static final int SWIPE_MAX_OFF_PATH = 200;
        private static final int SWIPE_THRESHOLD_VELOCITY = 100;

        @Override
        public boolean onFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) {
            try {
                float diffAbs = Math.abs(e1.getY() - e2.getY());
                float diff = e1.getX() - e2.getX();

                if (diffAbs > SWIPE_MAX_OFF_PATH)
                    return false;

                // Right swipe
                if (-diff > SWIPE_MIN_DISTANCE
                        && Math.abs(velocityX) > SWIPE_THRESHOLD_VELOCITY) {
                    Settings.this.onRightSwipe();
                }
            } catch (Exception e) {
                Log.e("I", "Error on gestures");
            }
            return false;
        }
    }

    public void showAlert(String message, String title) {
        final AlertDialog.Builder myAlert = new AlertDialog.Builder(this);
        myAlert.setMessage(message);
        myAlert.setPositiveButton("Ok", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                dialog.dismiss();
            }
        });
        myAlert.setTitle(title);
        myAlert.create();
        myAlert.show();
    }

    public void onBackPressed() {
        super.onBackPressed();
        int pid = android.os.Process.myPid();
        android.os.Process.killProcess(pid);
    }
}
