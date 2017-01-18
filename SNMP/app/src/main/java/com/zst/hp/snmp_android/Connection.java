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
        EditText textResponse;
        String oid = "1.3.6.1.2.1.1.1.0";
        String recieve_message = null;

    private static Connection instance = null;
    protected Connection() {
        // Exists only to defeat instantiation.
    }
    public static Connection getInstance() {
        if(instance == null) {
            instance = new Connection();
        }
        return instance;
    }

    Connection(String addr, int port, EditText Response) {
        dstAddress = addr;
        dstPort = port;
        this.textResponse = Response;
        Log.i("BBBB","Connection");
        
        }

@Override
protected String doInBackground(String... params)
        {
            Socket socket = null;
            try
            {
                Log.i("BBB", "Proba polaczenia z" + dstAddress + " na " + dstPort);
                socket = new Socket(dstAddress, dstPort);
                Log.i("BBB", "Poloczono");
                writer = new PrintStream(socket.getOutputStream());
                br = socket.getInputStream();
                //sendMessage("hi pc");

                byte[] buffer = new byte[4096];
                int read = br.read(buffer, 0, 4096); //This is blocking
                while(read != -1){
                    byte[] tempdata = new byte[read];
                    System.arraycopy(buffer, 0, tempdata, 0, read);
                    recieve_message = new String(tempdata);
                    Log.i("AsyncTask", recieve_message);
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
        writer.println(oid);
        writer.flush();
        Log.i("BBB", "wysylanie");
    }
    public String getRecieveMessage()
    {
        return recieve_message;
    }
@Override
protected void onPostExecute(String result) {
    if (result == null) {
        Log.e("007", "Something failed!");
    } else {
        Log.d("OO7", "In on post execute");
        textResponse.setText(result);
        super.onPostExecute(result);
    }
}
}


