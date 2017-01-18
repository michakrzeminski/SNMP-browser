package com.zst.hp.snmp_android;

import android.os.Bundle;

import android.util.Log;
import android.os.AsyncTask;
import android.widget.EditText;
import android.widget.TextView;

import android.util.Log;
import android.widget.Toast;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.Socket;
import java.io.ByteArrayOutputStream;
import java.net.UnknownHostException;
import java.io.OutputStreamWriter;
import java.io.BufferedOutputStream;
import java.util.Arrays;
import java.io.PrintStream;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;



public class Connection extends AsyncTask<String, String, String> {

        PrintStream writer;
        InputStreamReader reader;
        InputStream br;
        String dstAddress;
        int dstPort;
        String response = "";
        String receive_message = null;

    Connection(String addr, int port) {
        dstAddress = addr;
        dstPort = port;
        }

@Override
protected String doInBackground(String... params)
        {
            Socket socket = null;
            try
            {
                Log.i("I", "Connecting with: " + dstAddress + " on " + dstPort+" ...");
                socket = new Socket(dstAddress, dstPort);
                Log.i("I", "Connected");
                snmp.connected = true;

                writer = new PrintStream(socket.getOutputStream());
                br = socket.getInputStream();

                byte[] buffer = new byte[4096];
                int read = br.read(buffer, 0, 4096); //This is blocking
                while(read != -1){
                    byte[] tempdata = new byte[read];
                    System.arraycopy(buffer, 0, tempdata, 0, read);
                    receive_message = new String(tempdata);
                    Log.i("I", "Received: "+receive_message);
                    read = br.read(buffer, 0, 4096); //This is blocking
                }

            } catch (Exception e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
                response = "UnknownHostException: " + e.toString();
            } finally
            {
                if (socket != null)
                {
                    try {
                        writer.flush();
                        writer.close();
                        br.close();
                        socket.close();
                    } catch (IOException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                    }
                }
            }
        return null;
        }


    public void sendMessage(String oid)
    {
        oid+=".0";
        writer.println(oid);
        writer.flush();
        Log.i("I", "Sending: "+oid);
    }
    public String getReceiveMessage()
    {
        String temp = receive_message;
        receive_message = null;
        return temp;
    }
@Override
protected void onPostExecute(String result) {
    if (result == null) {
        Log.e("I", "Something failed!");
    } else {
        Log.d("I", "In on post execute");
        super.onPostExecute(result);
    }
}
}


