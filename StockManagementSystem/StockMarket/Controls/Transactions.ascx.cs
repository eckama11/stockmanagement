﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

using Common;
using DatabaseAccess;


public partial class Company_CompanyControls_BuyStock : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            SetActiveTab(lbtnPending);
            BindData("Pending");
            Session["TransType"] = "Pending";
        }
    }

    /// <summary>
    /// Bind the data of the gridview
    /// </summary>
    /// <param name="status">The status of the transaction to pull data from</param>
    private void BindData(string status)
    {
        gvTransactions.DataSource = StockMarket.GetTransactionDetailsByStatus(status);
        gvTransactions.DataBind();
    }

    /// <summary>
    /// Pagination for transactions
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvTransactions_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvTransactions.PageIndex = e.NewPageIndex;
        BindData(Session["TransType"].ToString());
    }

    /// <summary>
    /// The event to fire when selecting a row in the GridView
    /// </summary>
    protected void gvTransactions_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Reset each color back to the default value
        foreach (GridViewRow r in gvTransactions.Rows)
        {
            r.BackColor = Color.WhiteSmoke;
        }

        GridViewRow row = gvTransactions.SelectedRow;
        row.BackColor = Color.Turquoise;
        int id = int.Parse(gvTransactions.DataKeys[row.RowIndex].Value.ToString());
        string ticker = row.Cells[3].Text;
        int quantityRequested = int.Parse(row.Cells[4].Text);
        double price = double.Parse(row.Cells[5].Text);
        string type = row.Cells[6].Text;

        // Get the current quantity available for the stock chosen in the table
        int quantityAvailable = StockMarket.GetQuantityAvailable(ticker);

        // Set the labels in the Approve/Disapprove table to reflect the details of the selected transaction
        lblID.Text = id.ToString();
        lblTicker.Text = ticker;
        lblQuantity.Text = quantityRequested.ToString();
        lblQuantityAvailable.Text = quantityAvailable.ToString();
        lblPrice.Text = price.ToString();

        // Don't worry about warning the user about quantity if the transaction type is 'Sell'
        if (type != "Sell")
        {
            // Check if the quantity being requested is greater than the amount available
            if (quantityRequested > quantityAvailable)
            {
                lblQuantity.ForeColor = Color.Red;
                lblWarning.Visible = true;
            }
            else
            {
                lblQuantity.ForeColor = Color.Blue;
                lblWarning.Visible = false;
            }
        }

        //if lblWarning.Visible set to true previously
        //sell should reset lblWarning.Visible to false
        if (type == "Sell")
        {
            lblQuantity.ForeColor = Color.Blue;
            lblWarning.Visible = false;
        }

    }

    /// <summary>
    /// Approve a transaction on button click
    /// </summary>
    protected void btnApprove_Click(object sender, EventArgs e)
    {
        UpdateTransaction(int.Parse(lblID.Text), Enums.Status.Approved);
    }

    /// <summary>
    /// Disapprove a transaction on button click
    /// </summary>
    protected void btnDisapprove_Click(object sender, EventArgs e)
    {
        UpdateTransaction(int.Parse(lblID.Text), Enums.Status.Denied);
    }

    /// <summary>
    /// Put a transaction on hold
    /// </summary>
    protected void btnOnHold_Click(object sender, EventArgs e)
    {
        UpdateTransaction(int.Parse(lblID.Text), Enums.Status.OnHold);
    }

    /// <summary>
    /// Get the current user ID from a transaction
    /// </summary>
    /// <returns></returns>
    private int GetUserIdFromTransaction()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "SELECT user_id FROM Transactions WHERE transaction_id = '" + lblID.Text + "'";
        return int.Parse(SqlHelper.ExecuteScalar(cmd, Settings.StockMarketConn));

    }

    /// <summary>
    /// When the Approved linked button is fired, 
    /// update the table to show approved transactions.
    /// </summary>
    protected void lbtnApproved_Click(object sender, EventArgs e)
    {
        SetActiveTab(lbtnApproved);
        BindData("Approved");
        Session["TransType"] = "Approved";
        pnlApproveDisapprove.Visible = false;
    }

    /// <summary>
    /// When the Denied linked button is fired, 
    /// update the table to show denied transactions.
    /// </summary>
    protected void lbtnDenied_Click(object sender, EventArgs e)
    {
        SetActiveTab(lbtnDenied);
        BindData("Denied");
        Session["TransType"] = "Denied";
        pnlApproveDisapprove.Visible = false;
    }

    /// <summary>
    /// When the Pending linked button is fired, 
    /// update the table to show pending transactions.
    /// </summary>
    protected void lbtnPending_Click(object sender, EventArgs e)
    {
        SetActiveTab(lbtnPending);
        BindData("Pending");
        Session["TransType"] = "Pending";
        pnlApproveDisapprove.Visible = true;
    }


    protected void lbtnOnHold_Click(object sender, EventArgs e)
    {
        SetActiveTab(lbtnOnHold);
        BindData("On Hold");
        Session["TransType"] = "On Hold";
        pnlApproveDisapprove.Visible = true;
    }

    /// <summary>
    /// Update a transaction by either approving or denying it.
    /// </summary>
    /// <param name="id">The ID of the transaction</param>
    /// <param name="status">The new status of the transaction</param>
    private void UpdateTransaction(int id, Enums.Status status)
    {
        int user_to = GetUserIdFromTransaction();
        // Make sure a transaction message is supplied
        if (txtMessage.Text.Length >= 10)
        {
            // Update transaction to be approved
            StockMarket.UpdateTransaction(id, status, txtMessage.Text);
            switch (status)
            {
                case Enums.Status.Approved:
                    StockMarket.InsertMessage(user_to, "A transaction you made has been approved. Please go to your transactions to see more details.");
                    break;
                case Enums.Status.Denied:
                    StockMarket.InsertMessage(user_to, "A transaction you made has been denied. Please go to your transactions to see more details.");
                    break;
                case Enums.Status.OnHold:
                    StockMarket.InsertMessage(user_to, "A transaction you made has been put on hold. Please go to your transactions to see more details.");
                    break;
                case Enums.Status.Pending:
                    break;
                default:
                    break;
            }

            ClearFields();
            BindData("Pending");
            Session["TransType"] = "Pending";
        }
        else
        {
            App.ShowAlertMessage("Your transaction message must have at least 10 characters");
        }
    }

    /// <summary>
    /// Reset all the labels to their default values.
    /// </summary>
    private void ClearFields()
    {
        lblID.Text = "****";
        lblTicker.Text = "****";
        lblQuantity.Text = "****";
        lblQuantityAvailable.Text = "****";
        lblPrice.Text = "****";
        txtMessage.Text = String.Empty;
    }

    /// <summary>
    /// Set the active link button
    /// </summary>
    /// <param name="lbtnStatus">The link button to set</param>
    private void SetActiveTab(LinkButton lbtnStatus)
    {
        lbtnPending.ForeColor = Color.SlateGray;
        lbtnApproved.ForeColor = Color.SlateGray;
        lbtnDenied.ForeColor = Color.SlateGray;
        lbtnOnHold.ForeColor = Color.SlateGray;
        lbtnStatus.ForeColor = Color.Blue;
    }


}