package com.zst.hp.snmp_android;

import android.app.Activity;
import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.GestureDetector;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import static com.zst.hp.snmp_android.R.id.editText;

public class Settings extends Activity {

    public EditText response;
    public  EditText send;
    public EditText editTextAddress, editTextPort;
    public Button buttonConnect;
    private GestureDetector gestureDetector;
    public Connection connection;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_settings);

        send = (EditText) findViewById(R.id.responseEditText);
        editTextAddress = (EditText) findViewById(R.id.editText);
        editTextPort = (EditText) findViewById(R.id.editText2);
        buttonConnect = (Button) findViewById(R.id.button);
        response = (EditText) findViewById(R.id.responseEditText);
        gestureDetector = new GestureDetector(new SwipeGestureDetector());
    }

    public void buttonOnClick(View v)
    {
        Log.i("AAAA","Wchodze do conection");
        connection = new Connection(editTextAddress.getText()
                .toString(), Integer.parseInt(editTextPort
                .getText().toString()), response);
       connection.execute();


        // Perform action on click


    }
    public void buttonSendClick(View v)
    {
        Log.i("AAAA","Wysylam");
        connection.sendMessage(send.getText().toString());
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
                Log.e("Snmp", "Error on gestures");
            }
            return false;
        }
    }

}
